using System;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using AutoStep.Compiler.Parser;

namespace AutoStep.Compiler
{
    internal class DefaultAutoStepErrorHandling : BaseAutoStepErrorHandlingContext
    {
        public DefaultAutoStepErrorHandling(ITokenStream tokenStream, IRecognizer recognizer, IToken offendingSymbol, RecognitionException ex)
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
                UnexpectedEndOfFile
            );
        }

        private bool FeatureTitleInputMismatchMissingTitle()
        {
            if (ContextIs<AutoStepParser.FeatureTitleContext>() &&
                ExpectingTokens(AutoStepParser.WORD) &&
                (OffendingSymbolIsNot(AutoStepParser.Eof) || ExceptionIs<InputMismatchException>()))
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
            if (ExceptionIs<InputMismatchException>() &&
                ContextIs<AutoStepParser.NormalScenarioTitleContext>() &&
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
            if (ExceptionIs<InputMismatchException>() &&
                ContextIs<AutoStepParser.ScenarioOutlineTitleContext>() &&
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
            if (ExceptionIs<InputMismatchException>() &&
                ContextIs<AutoStepParser.TableHeaderContext>())
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
