using System;
using System.Collections.Generic;
using System.Text;
using Antlr4.Runtime;
using AutoStep.Language.Interaction.Parser;

namespace AutoStep.Language.Interaction
{
    using static AutoStep.Language.Interaction.Parser.AutoStepInteractionsParser;

    internal class InteractionsErrorHandlingContext : BaseAutoStepErrorHandlingContext<AutoStepInteractionsParser>
    {
        public InteractionsErrorHandlingContext(ITokenStream tokenStream, IRecognizer recognizer, IToken offendingSymbol, RecognitionException? ex)
            : base(tokenStream, recognizer, offendingSymbol, ex)
        {
            SetErrorHandlers(
                NameMissingHandler
            );
        }

        private bool NameMissingHandler()
        {
            if (ExpectingTokens(NAME_REF)
                && (Context is ComponentDefinitionContext || Context is TraitDefinitionContext))
            {
                ChangeError(CompilerMessageCode.InteractionMissingExpectedName);

                UseOpeningTokenAsStart(COMPONENT_DEFINITION, TRAIT_DEFINITION);
                UseStartSymbolAsEndSymbol();

                SwallowEndOfFileErrorsAfterThis();

                return true;
            }

            return false;
        }
    }
}
