using Antlr4.Runtime;
using AutoStep.Language.Interaction.Parser;

namespace AutoStep.Language.Interaction
{
    using static AutoStep.Language.Interaction.Parser.AutoStepInteractionsParser;

    /// <summary>
    /// Error handling for the interactions language.
    /// </summary>
    internal class InteractionsErrorHandlingContext : BaseErrorHandlingContext<AutoStepInteractionsParser>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InteractionsErrorHandlingContext"/> class.
        /// </summary>
        /// <param name="tokenStream">The token stream.</param>
        /// <param name="recognizer">The recognizer.</param>
        /// <param name="offendingSymbol">The offending symbol.</param>
        /// <param name="ex">The exception (if available).</param>
        public InteractionsErrorHandlingContext(ITokenStream tokenStream, IRecognizer recognizer, IToken offendingSymbol, RecognitionException? ex)
            : base(tokenStream, recognizer, offendingSymbol, ex)
        {
            SetErrorHandlers(
                NameMissingHandler,
                MissingStepDeclaration,
                MissingMethodParentheses,
                MissingMethodDeclArgSeparator,
                MethodDeclInvalidParameterStringDeclaration,
                MethodDeclInvalidParameterDeclaration,
                CallChainMissingSeparator,
                MissingMethodCallArgSeparator,
                UnterminatedMethod,
                MethodCallUnterminatedString,
                MissingMethodDeclArgument);
        }

        private bool NameMissingHandler()
        {
            if (ExpectingTokens(NAME_REF)
                && (Context is ComponentDefinitionContext || Context is TraitDefinitionContext || Context is ComponentInheritsContext))
            {
                ChangeError(CompilerMessageCode.InteractionMissingExpectedName);

                UseOpeningTokenAsStart(COMPONENT_DEFINITION, TRAIT_DEFINITION, INHERITS_KEYWORD);
                UseStartSymbolAsEndSymbol();

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

                return true;
            }

            return false;
        }

        private bool MissingMethodDeclArgSeparator()
        {
            if (ExpectingTokens(METHOD_CLOSE) && OffendingSymbolIs(PARAM_NAME)
                && Context is MethodDeclarationContext)
            {
                ChangeError(CompilerMessageCode.InteractionMethodDeclMissingParameterSeparator);

                return true;
            }

            return false;
        }

        private bool MissingMethodCallArgSeparator()
        {
            if (ExpectingTokens(METHOD_CLOSE)
                && Context is MethodCallContext && OffendingSymbolIsOneOf(
                    FLOAT,
                    INT,
                    METHOD_STRING_START,
                    PARAM_NAME,
                    CONSTANT))
            {
                ChangeError(CompilerMessageCode.InteractionMethodCallMissingParameterSeparator);

                return true;
            }

            return false;
        }

        private bool MethodDeclInvalidParameterStringDeclaration()
        {
            if (ExpectingTokens(PARAM_NAME) && OffendingSymbolIs(METHOD_STRING_START)
                && Context is MethodDefArgsContext)
            {
                while (Parser.CurrentToken.Type != METHOD_STRING_END &&
                       Parser.CurrentToken.Type != METHOD_STRING_ERRNL &&
                       Parser.CurrentToken.Type != AutoStepInteractionsLexer.Eof)
                {
                    Parser.Consume();
                }

                if (Parser.CurrentToken.Type == METHOD_STRING_END)
                {
                    ChangeError(CompilerMessageCode.InteractionMethodDeclUnexpectedContent);

                    UseTokenAsEnd(Parser.CurrentToken);
                }
                else
                {
                    ChangeError(CompilerMessageCode.InteractionUnterminatedString);

                    UsePrecedingTokenAsEnd(Parser.CurrentToken, STR_CONTENT);
                }

                return true;
            }

            return false;
        }

        private bool MethodCallUnterminatedString()
        {
            if (Context is StringArgContext && OffendingSymbolIsOneOf(METHOD_STRING_ERRNL, Lexer.Eof))
            {
                ChangeError(CompilerMessageCode.InteractionUnterminatedString);

                UseOpeningTokenAsStart(STR_CONTENT);
                UsePrecedingTokenAsEnd(Parser.CurrentToken, STR_CONTENT);

                return true;
            }

            return false;
        }

        private bool MethodDeclInvalidParameterDeclaration()
        {
            if (ExpectingTokens(PARAM_NAME) && OffendingSymbolIsOneOf(CONSTANT, ARR_LEFT, ARR_RIGHT, FLOAT, INT)
                && Context is MethodDefArgsContext)
            {
                ChangeError(CompilerMessageCode.InteractionMethodDeclUnexpectedContent);

                return true;
            }

            return false;
        }

        private bool UnterminatedMethod()
        {
            if (ExpectingTokens(METHOD_CLOSE))
            {
                if (Context is MethodDeclarationContext)
                {
                    ChangeError(CompilerMessageCode.InteractionMethodDeclUnterminated);

                    return true;
                }

                if (Context is MethodCallContext)
                {
                    ChangeError(CompilerMessageCode.InteractionMethodCallUnterminated);

                    UseOpeningTokenAsStart(NAME_REF);

                    UsePrecedingTokenAsEnd();

                    return true;
                }
            }

            return false;
        }

        private bool MissingMethodDeclArgument()
        {
            if (ExpectingTokens(PARAM_NAME) && OffendingSymbolIs(METHOD_CLOSE)
                && (Context is MethodDefArgsContext))
            {
                ChangeError(CompilerMessageCode.InteractionMethodDeclMissingParameter);

                UseOpeningTokenAsStart(PARAM_SEPARATOR);

                return true;
            }

            return false;
        }

        private bool CallChainMissingSeparator()
        {
            if (Exception is CallChainSeparationException)
            {
                // Missing Separator.
                ChangeError(CompilerMessageCode.InteractionMethodCallMissingSeparator);

                return true;
            }

            return false;
        }
    }
}
