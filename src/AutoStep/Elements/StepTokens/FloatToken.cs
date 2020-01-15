namespace AutoStep.Elements.StepTokens
{
    /// <summary>
    /// Represents a float token (actually backed by a decimal).
    /// </summary>
    internal class FloatToken : NumericalToken<decimal>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FloatToken"/> class.
        /// </summary>
        /// <param name="startIndex">The starting 0-based index within the text.</param>
        /// <param name="length">The character length of the token.</param>
        public FloatToken(int startIndex, int length)
            : base(startIndex, length)
        {
        }
    }
}
