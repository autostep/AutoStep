namespace AutoStep.Elements.StepTokens
{
    /// <summary>
    /// Represents an integer token (actually backed by a long).
    /// </summary>
    internal class IntToken : NumericalToken<long>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IntToken"/> class.
        /// </summary>
        /// <param name="startIndex">The starting 0-based index within the text.</param>
        /// <param name="length">The character length of the token.</param>
        public IntToken(int startIndex, int length)
            : base(startIndex, length)
        {
        }
    }
}
