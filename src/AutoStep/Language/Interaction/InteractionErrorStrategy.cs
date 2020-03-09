using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Misc;
using AutoStep.Language.Interaction.Parser;
using AutoStep.Language.Interaction.Visitors;
using AutoStep.Language.Test;
using AutoStep.Language.Test.Parser;
using Microsoft.Extensions.Logging;

namespace AutoStep.Language.Interaction
{
    using static AutoStepInteractionsParser;

    class InteractionErrorStrategy : DefaultErrorStrategy
    {
        // Defines the expected set of tokens that exist inside the 
        private readonly IntervalSet insideMethodSet = new IntervalSet(
            METHOD_OPEN,
            METHOD_STRING_START,
            METHOD_STRING_END,
            PARAM_NAME,
            ARR_LEFT,
            ARR_RIGHT,
            CONSTANT,
            INT,
            FLOAT,
            STR_CONTENT,
            METHOD_STR_ESCAPE_QUOTE,
            STR_ANGLE_LEFT,
            STR_NAME_REF,
            STR_ANGLE_RIGHT
        );

        public override void Sync(Antlr4.Runtime.Parser recognizer)
        {
            if (recognizer.Context is MethodCallChainContext)
            {
                SyncMethodCallChain(recognizer);
            }

            base.Sync(recognizer);
        }

        private void SyncMethodCallChain(Antlr4.Runtime.Parser recognizer)
        {
            // The purpose of this method is to correctly diagnose any missing call separators.

            // Inside a call chain, we want to see what's missing.
            var state = recognizer.Interpreter.atn.states[recognizer.State];
            var tokens = (ITokenStream)recognizer.InputStream;
            int la = tokens.LA(1);

            var nextTokens = recognizer.Atn.NextTokens(state);

            // If next tokens expects a FUNC_PASS_MARKER, but all we have is a NAME_REF, lets
            // look a little further ahead.
            // If that NAME_REF is followed by a method close, then a def separator, we can safely assume
            // we have changed method chain, and we're good. Otherwise, we are missing a func pass.
            while (la == NAME_REF && nextTokens.Contains(FUNC_PASS_MARKER))
            {
                var position = 2;

                // Search the token stream for the end of the method.
                do
                {
                    la = tokens.LA(position);
                    position++;
                } while (insideMethodSet.Contains(la));

                // If we've existed the method 'cleanly', i.e. with a
                // ')', then check what comes afterwards.
                if (la == METHOD_CLOSE)
                {
                    // Next after method close.
                    la = tokens.LA(position);

                    if (la != DEF_SEPARATOR)
                    {
                        ReportError(recognizer, new CallChainSeparationException(recognizer));

                        var consume = 1;

                        // Move past this bit.
                        while (consume < position)
                        {
                            recognizer.Consume();
                            consume++;
                        }

                        state = recognizer.Interpreter.atn.states[recognizer.State];
                        la = tokens.LA(1);
                        nextTokens = recognizer.Atn.NextTokens(state);

                        EndErrorCondition(recognizer);
                    }
                }
                else
                {
                    break;
                }
            }
        }

        protected override bool SingleTokenInsertion(Antlr4.Runtime.Parser recognizer)
        {
            // We need to be able to curtail the attempts by ANTLR to recover from errors where we've experienced an unterminated string
            // inside the current context. So, if we have encountered an unterminated string line marker, we will just bail from the rule.
            if (recognizer.Context is MethodDefinitionContext)
            {
                var searchStartIndex = recognizer.Context.Start.TokenIndex;
                var currentIndex = recognizer.TokenStream.Index;

                // Work backwards, looking for a safe 'sequencing' point, where we know ANTLR can recover sensibly.
                while (currentIndex > searchStartIndex)
                {
                    var token = recognizer.TokenStream.Get(currentIndex);

                    if (token.Type == FUNC_PASS_MARKER)
                    {
                       // We've reached a 'safe' point.
                       break;
                    }
                    else if (token.Type == METHOD_STRING_ERRNL)
                    {
                        // Not safe to recover.
                        throw new InputMismatchException(recognizer);
                    }

                    currentIndex--;
                }
            }

            return base.SingleTokenInsertion(recognizer);
        }
    }
}
