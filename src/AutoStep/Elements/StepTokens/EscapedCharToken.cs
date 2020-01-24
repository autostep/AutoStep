namespace AutoStep.Elements.StepTokens
{
    /// <summary>
    /// Represents an escape character within the step reference.
    /// </summary>
    internal class EscapedCharToken : StepToken
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EscapedCharToken"/> class.
        /// </summary>
        /// <param name="escapedValue">The escaped character value.</param>
        /// <param name="startIndex">The start index.</param>
        /// <param name="length">The length of the character token.</param>
        public EscapedCharToken(string escapedValue, int startIndex, int length)
            : base(startIndex, length)
        {
            // If the length is ever not 2 characters (i.e. backslash followed by character), then the parser has gone wrong.
            if (length != 2)
            {
                throw new LanguageEngineAssertException();
            }

            EscapedValue = escapedValue;
        }

        /// <summary>
        /// Gets the escaped character value.
        /// </summary>
        public string EscapedValue { get; }
    }
}
