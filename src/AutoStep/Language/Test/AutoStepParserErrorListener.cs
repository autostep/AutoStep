using Antlr4.Runtime;
using AutoStep.Language.Test.Parser;

namespace AutoStep.Language.Test
{
    /// <summary>
    /// AutoStep Test Language error listener.
    /// </summary>
    internal class AutoStepParserErrorListener : ParserErrorListener<AutoStepParser>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AutoStepParserErrorListener"/> class.
        /// </summary>
        /// <param name="sourceName">The source name.</param>
        /// <param name="tokenStream">The token stream.</param>
        public AutoStepParserErrorListener(string? sourceName, ITokenStream tokenStream)
            : base(sourceName, tokenStream)
        {
        }

        /// <inheritdoc/>
        protected override BaseAutoStepErrorHandlingContext<AutoStepParser> CreateErrorContext(IRecognizer recognizer, IToken offendingSymbol, RecognitionException e)
        {
            return new DefaultAutoStepErrorHandling(TokenStream, recognizer, offendingSymbol, e);
        }
    }
}
