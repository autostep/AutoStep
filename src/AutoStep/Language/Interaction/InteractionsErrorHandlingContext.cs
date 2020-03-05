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
                NameMissingHandler,
                MissingStepDeclaration,
                MissingMethodParentheses
            );
        }

        private bool NameMissingHandler()
        {
            if (ExpectingTokens(NAME_REF)
                && (Context is ComponentDefinitionContext || Context is TraitDefinitionContext || Context is ComponentInheritsContext))
            {
                ChangeError(CompilerMessageCode.InteractionMissingExpectedName);

                UseOpeningTokenAsStart(COMPONENT_DEFINITION, TRAIT_DEFINITION, INHERITS_KEYWORD);
                UseStartSymbolAsEndSymbol();

                SwallowEndOfFileErrorsAfterThis();

                return true;
            }

            return false;
        }

        private bool MissingStepDeclaration()
        {
            if (ExpectingTokens(DEF_GIVEN, DEF_WHEN, DEF_THEN)
                && Context is StepDefinitionContext)
            {
                ChangeError(CompilerMessageCode.InteractionMissingStepDeclaration);

                UseOpeningTokenAsStart(STEP_DEFINE);
                UseStartSymbolAsEndSymbol();

                SwallowEndOfFileErrorsAfterThis();

                return true;
            }

            return false;
        }

        private bool MissingMethodParentheses()
        {
            if (ExpectingTokens(METHOD_OPEN)
                && (Context is MethodDeclarationContext || Context is MethodCallContext))
            {
                ChangeError(CompilerMessageCode.InteractionMethodNeedsParentheses);

                UseOpeningTokenAsStart(NAME_REF);

                UseStartSymbolAsEndSymbol();

                SwallowEndOfFileErrorsAfterThis();

                return true;
            }

            return false;
        }
    }
}
