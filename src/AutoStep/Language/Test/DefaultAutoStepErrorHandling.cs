using System;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using AutoStep.Language.Test.Parser;

namespace AutoStep.Language.Test
{
    using static AutoStepParser;

    /// <summary>
    /// Provides the default syntax error handling.
    /// </summary>
    internal class DefaultAutoStepErrorHandling : BaseAutoStepErrorHandlingContext<AutoStepParser>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultAutoStepErrorHandling"/> class.
        /// </summary>
        /// <param name="tokenStream">The token stream for the file being parsed.</param>
        /// <param name="recognizer">The Antlr recogniser.</param>
        /// <param name="offendingSymbol">The offending symbol from the syntax error.</param>
        /// <param name="ex">The syntax parse exception (if there is one).</param>
        public DefaultAutoStepErrorHandling(ITokenStream tokenStream, IRecognizer recognizer, IToken offendingSymbol, RecognitionException? ex)
            : base(tokenStream, recognizer, offendingSymbol, ex)
        {
            SetErrorHandlers(
                ScenarioTitleInputMismatchMissingTitle,
                ScenarioOutlineTitleInputMismatchMissingTitle,
                DefinitionInvalidStepType,
                DefinitionEmptyArgument,
                DefinitionArgumentWhitespace,
                InvalidTagDefinitionExpectingWord,
                InvalidOptionDefinitionExpectingWord,
                FeatureTitleInputMismatchMissingTitle,
                ExpectingTableRowTerminator,
                ExampleBlockInputMismatchExpectingTable,
                UnexpectedEndOfFile);
        }

        private bool FeatureTitleInputMismatchMissingTitle()
        {
            if (Context is FeatureTitleContext &&
                ExpectingTokens(WORD) &&
                (OffendingSymbolIsNot(AutoStepParser.Eof) || Exception is InputMismatchException))
            {
                ChangeError(CompilerMessageCode.NoFeatureTitleProvided);

                UseOpeningTokenAsStart(FEATURE);
                UseStartSymbolAsEndSymbol();

                SwallowEndOfFileErrorsAfterThis();

                return true;
            }

            return false;
        }

        private bool ScenarioTitleInputMismatchMissingTitle()
        {
            if (Exception is InputMismatchException &&
                Context is NormalScenarioTitleContext &&
                ExpectingTokens(WORD))
            {
                // Title should have been provided, but it has not.
                ChangeError(CompilerMessageCode.NoScenarioTitleProvided);

                UseOpeningTokenAsStart(SCENARIO);
                UseStartSymbolAsEndSymbol();

                return true;
            }

            return false;
        }

        private bool DefinitionInvalidStepType()
        {
            if (Exception is InputMismatchException &&
                Context is StepDeclarationContext &&
                !ContextStartedWithOneOf(DEF_GIVEN, DEF_WHEN, DEF_THEN) &&
                ExpectingTokens(DEF_GIVEN, DEF_WHEN, DEF_THEN))
            {
                ChangeError(CompilerMessageCode.InvalidStepDefineKeyword, OffendingSymbol.Text);

                return true;
            }

            return false;
        }

        private bool DefinitionEmptyArgument()
        {
            if (Context is StepDeclarationArgumentNameContext &&
                ExpectingTokens(DEF_WORD))
            {
                ChangeError(CompilerMessageCode.StepVariableNameRequired);

                UseOpeningTokenAsStart(DEF_LCURLY);

                return true;
            }

            return false;
        }

        private bool DefinitionArgumentWhitespace()
        {
            if (Context is DeclarationArgumentContext &&
                ExpectingTokens(DEF_RCURLY, DEF_WORD) && OffendingSymbolIs(DEF_WS))
            {
                ChangeError(CompilerMessageCode.StepVariableInvalidWhitespace);

                return true;
            }

            return false;
        }

        private bool ScenarioOutlineTitleInputMismatchMissingTitle()
        {
            if (Exception is InputMismatchException &&
                Context is ScenarioOutlineTitleContext &&
                ExpectingTokens(WORD))
            {
                // Title should have been provided, but it has not.
                ChangeError(CompilerMessageCode.NoScenarioOutlineTitleProvided);

                UseOpeningTokenAsStart(SCENARIO_OUTLINE);

                UseStartSymbolAsEndSymbol();

                return true;
            }

            return false;
        }

        private bool UnexpectedEndOfFile()
        {
            if (OffendingSymbolIs(AutoStepParser.Eof))
            {
                ChangeError(CompilerMessageCode.UnexpectedEndOfFile);
                return true;
            }

            return false;
        }

        private bool InvalidTagDefinitionExpectingWord()
        {
            if (OffendingSymbolIs(WORD) &&
                OffendingSymbolTextIs("@"))
            {
                ChangeError(CompilerMessageCode.BadTagFormat);
                return true;
            }

            return false;
        }

        private bool InvalidOptionDefinitionExpectingWord()
        {
            if (OffendingSymbolIs(WORD) &&
                OffendingSymbolTextIs("$"))
            {
                ChangeError(CompilerMessageCode.BadOptionFormat);
                return true;
            }

            return false;
        }

        private bool ExpectingTableRowTerminator()
        {
            if (OffendingSymbolIs(ROW_NL))
            {
                ChangeError(CompilerMessageCode.TableRowHasNotBeenTerminated);
                UseOpeningTokenAsStart(TABLE_START, CELL_DELIMITER);
                UsePrecedingTokenAsEnd();

                return true;
            }

            return false;
        }

        private bool ExampleBlockInputMismatchExpectingTable()
        {
            if (Exception is InputMismatchException &&
                Context is TableHeaderContext)
            {
                ChangeError(CompilerMessageCode.ExamplesBlockRequiresTable);

                UseOpeningTokenAsStart(EXAMPLES);
                UseStartSymbolAsEndSymbol();

                return true;
            }

            return false;
        }
    }
}
