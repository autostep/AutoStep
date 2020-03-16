using Antlr4.Runtime;
using AutoStep.Language.Test.Parser;

namespace AutoStep.Language.Test
{
    /// <summary>
    /// AutoStep Test Language error listener.
    /// </summary>
    internal class TestParserErrorListener : ParserErrorListener<AutoStepParser>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestParserErrorListener"/> class.
        /// </summary>
        /// <param name="sourceName">The source name.</param>
        /// <param name="tokenStream">The token stream.</param>
        public TestParserErrorListener(string? sourceName, ITokenStream tokenStream)
            : base(sourceName, tokenStream)
        {
        }

        /// <inheritdoc/>
        protected override BaseErrorHandlingContext<AutoStepParser> CreateErrorContext(IRecognizer recognizer, IToken offendingSymbol, RecognitionException e)
        {
            return new DefaultTestErrorHandling(TokenStream, recognizer, offendingSymbol, e);
        }
    }
}
