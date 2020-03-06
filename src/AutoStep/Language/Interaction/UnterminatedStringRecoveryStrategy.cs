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

    class UnterminatedStringRecoveryStrategy : DefaultErrorStrategy
    {
        protected override bool SingleTokenInsertion(Antlr4.Runtime.Parser recognizer)
        {
            // We need to be able to curtail the attempts by ANTLR to recover from errors where we've experienced an unterminated string
            // inside the current context. So, if we have encountered an unterminated string line marker, we will just bail from the rule.
            if (recognizer.Context is AutoStepInteractionsParser.MethodDefinitionContext)
            {
                var searchStartIndex = recognizer.Context.Start.TokenIndex;
                var currentIndex = recognizer.TokenStream.Index;

                // Work backwards, looking for a safe 'sequencing' point, where we know ANTLR can recover sensibly.
                while (currentIndex > searchStartIndex)
                {
                    var token = recognizer.TokenStream.Get(currentIndex);

                    if (token.Type == AutoStepInteractionsParser.FUNC_PASS_MARKER)
                    {
                       // We've reached a 'safe' point.
                       break;
                    }
                    else if (token.Type == AutoStepInteractionsParser.METHOD_STRING_ERRNL)
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
