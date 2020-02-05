namespace AutoStep.Elements.StepTokens
{
    /// <summary>
    /// Defines the notion of a step token; a range of characters within the defined step text. Similar to the internal Antlr tokens, but slightly simpler,
    /// and prevents us holding on to the Antlr parse tree memory too long.
    /// </summary>
    internal abstract class StepToken : PositionalElement
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StepToken"/> class.
        /// </summary>
        /// <param name="startIndex">The starting 0-based index within the text.</param>
        /// <param name="length">The character length of the token.</param>
        protected StepToken(int startIndex, int length)
        {
            StartIndex = startIndex;
            Length = length;
        }

        /// <summary>
        /// Gets the starting 0-based index within the text.
        /// </summary>
        public int StartIndex { get; }

        /// <summary>
        /// Gets the character length of the token.
        /// </summary>
        public int Length { get; }

        /// <summary>
        /// Checks if the specified token occurs immediately after this token in the text stream.
        /// </summary>
        /// <param name="nextToken">The token to check.</param>
        /// <returns>True if the specified part immediately follows this one.</returns>
        public bool IsImmediatelyFollowedBy(StepToken nextToken)
        {
            return StartIndex + Length == nextToken.StartIndex;
        }
    }
}
