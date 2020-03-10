using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Antlr4.Runtime;
using AutoStep.Language.Test.Parser;

namespace AutoStep.Language.Test
{
    internal class AutoStepParserErrorListener : ParserErrorListener<AutoStepParser>
    {
        public AutoStepParserErrorListener(string? sourceName, ITokenStream tokenStream)
            : base(sourceName, tokenStream)
        {
        }

        protected override BaseAutoStepErrorHandlingContext<AutoStepParser> CreateErrorContext(IRecognizer recognizer, IToken offendingSymbol, RecognitionException e)
        {
            return new DefaultAutoStepErrorHandling(TokenStream, recognizer, offendingSymbol, e);
        }
    }
}
