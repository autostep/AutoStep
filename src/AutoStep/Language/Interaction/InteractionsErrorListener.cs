using Antlr4.Runtime;
using AutoStep.Language.Interaction.Parser;

namespace AutoStep.Language.Interaction
{
    /// <summary>
    /// Interactions Error Listener. Creates error handling contexts.
    /// </summary>
    internal class InteractionsErrorListener : ParserErrorListener<AutoStepInteractionsParser>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InteractionsErrorListener"/> class.
        /// </summary>
        /// <param name="sourceName">The source file name.</param>
        /// <param name="tokenStream">The token stream.</param>
        public InteractionsErrorListener(string? sourceName, ITokenStream tokenStream)
            : base(sourceName, tokenStream)
        {
        }

        /// <inheritdoc/>
        protected override BaseErrorHandlingContext<AutoStepInteractionsParser> CreateErrorContext(IRecognizer recognizer, IToken offendingSymbol, RecognitionException e)
        {
            return new InteractionsErrorHandlingContext(TokenStream, recognizer, offendingSymbol, e);
        }
    }
}
