using System;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using AutoStep.Compiler.Parser;

namespace AutoStep.Compiler
{
    /// <summary>
    /// Provides the default syntax error handling.
    /// </summary>
    internal class DefaultAutoStepErrorHandling : BaseAutoStepErrorHandlingContext
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
                InvalidTagDefinitionExpectingWord,
                InvalidOptionDefinitionExpectingWord,
                FeatureTitleInputMismatchMissingTitle,
                ExpectingArgumentClosingQuote,
                ExpectingTableRowTerminator,
                ExampleBlockInputMismatchExpectingTable,
                UnexpectedEndOfFile);
        }

        private bool FeatureTitleInputMismatchMissingTitle()
        {
            if (Context is AutoStepParser.FeatureTitleContext &&
                ExpectingTokens(AutoStepParser.WORD) &&
                (OffendingSymbolIsNot(AutoStepParser.Eof) || Exception is InputMismatchException))
            {
                ChangeError(CompilerMessageCode.NoFeatureTitleProvided);

                UseOpeningTokenAsStart(AutoStepParser.FEATURE);
                UseStartSymbolAsEndSymbol();

                SwallowEndOfFileErrorsAfterThis();

                return true;
            }

            return false;
        }

        private bool ScenarioTitleInputMismatchMissingTitle()
        {
            if (Exception is InputMismatchException &&
                Context is AutoStepParser.NormalScenarioTitleContext &&
                ExpectingTokens(AutoStepParser.WORD))
            {
                // Title should have been provided, but it has not.
                ChangeError(CompilerMessageCode.NoScenarioTitleProvided);

                UseOpeningTokenAsStart(AutoStepParser.SCENARIO);
                UseStartSymbolAsEndSymbol();

                return true;
            }

            return false;
        }

        private bool ScenarioOutlineTitleInputMismatchMissingTitle()
        {
            if (Exception is InputMismatchException &&
                Context is AutoStepParser.ScenarioOutlineTitleContext &&
                ExpectingTokens(AutoStepParser.WORD))
            {
                // Title should have been provided, but it has not.
                ChangeError(CompilerMessageCode.NoScenarioOutlineTitleProvided);

                UseOpeningTokenAsStart(AutoStepParser.SCENARIO_OUTLINE);

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
            if (OffendingSymbolIs(AutoStepParser.WORD) &&
                OffendingSymbolTextIs("@"))
            {
                ChangeError(CompilerMessageCode.BadTagFormat);
                return true;
            }

            return false;
        }

        private bool InvalidOptionDefinitionExpectingWord()
        {
            if (OffendingSymbolIs(AutoStepParser.WORD) &&
                OffendingSymbolTextIs("$"))
            {
                ChangeError(CompilerMessageCode.BadOptionFormat);
                return true;
            }

            return false;
        }

        private bool ExpectingArgumentClosingQuote()
        {
            if (OffendingSymbolIs(AutoStepParser.ARG_ERR_UNEXPECTEDTERMINATOR))
            {
                ChangeError(CompilerMessageCode.ArgumentHasNotBeenClosed);

                UseOpeningTokenAsStart(AutoStepParser.OPEN_QUOTE);
                UsePrecedingTokenAsEnd();

                return true;
            }

            return false;
        }

        private bool ExpectingTableRowTerminator()
        {
            if (OffendingSymbolIs(AutoStepParser.ROW_NL))
            {
                ChangeError(CompilerMessageCode.TableRowHasNotBeenTerminated);
                UseOpeningTokenAsStart(AutoStepParser.TABLE_START, AutoStepParser.CELL_DELIMITER);
                UsePrecedingTokenAsEnd();

                return true;
            }

            return false;
        }

        private bool ExampleBlockInputMismatchExpectingTable()
        {
            if (Exception is InputMismatchException &&
                Context is AutoStepParser.TableHeaderContext)
            {
                ChangeError(CompilerMessageCode.ExamplesBlockRequiresTable);

                UseOpeningTokenAsStart(AutoStepParser.EXAMPLES);
                UseStartSymbolAsEndSymbol();

                return true;
            }

            return false;
        }
    }
}
