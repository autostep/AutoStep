namespace AutoStep.Elements.StepTokens
{
    /// <summary>
    /// Represents a single or double quote.
    /// </summary>
    internal class QuoteToken : StepToken
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QuoteToken"/> class.
        /// </summary>
        /// <param name="isDoubleQuote">Indicates whether the token is a double quote.</param>
        /// <param name="startIndex">The position of the quote within the text.</param>
        public QuoteToken(bool isDoubleQuote, int startIndex)
            : base(startIndex, 1)
        {
            IsDoubleQuote = isDoubleQuote;
        }

        /// <summary>
        /// Gets a value indicating whether this token is a double quote ("), rather than single (').
        /// </summary>
        public bool IsDoubleQuote { get; }
    }
}
