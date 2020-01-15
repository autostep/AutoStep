namespace AutoStep.Elements.StepTokens
{
    /// <summary>
    /// Represents a word in a matched step reference. Words are text sequences.
    /// </summary>
    internal class TextToken : StepToken
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TextToken"/> class.
        /// </summary>
        /// <param name="startIndex">The start index in the step text.</param>
        /// <param name="length">The length of the token in the step text.</param>
        public TextToken(int startIndex, int length)
            : base(startIndex, length)
        {
        }
    }
}
