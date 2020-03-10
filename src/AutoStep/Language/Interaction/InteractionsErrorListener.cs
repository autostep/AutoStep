using System;
using System.Collections.Generic;
using System.Text;
using Antlr4.Runtime;
using AutoStep.Language.Interaction.Parser;

namespace AutoStep.Language.Interaction
{

    internal class InteractionsErrorListener : ParserErrorListener<AutoStepInteractionsParser>
    {
        public InteractionsErrorListener(string? sourceName, ITokenStream tokenStream)
            : base(sourceName, tokenStream)
        {
        }

        protected override BaseAutoStepErrorHandlingContext<AutoStepInteractionsParser> CreateErrorContext(IRecognizer recognizer, IToken offendingSymbol, RecognitionException e)
        {
            return new InteractionsErrorHandlingContext(TokenStream, recognizer, offendingSymbol, e);
        }
    }
}
