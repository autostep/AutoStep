namespace AutoStep.Elements.StepTokens
{
    /// <summary>
    /// Represents a variable insertion token.
    /// </summary>
    internal class VariableToken : StepToken
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VariableToken"/> class.
        /// </summary>
        /// <param name="variableName">The name of the matched variable.</param>
        /// <param name="startIndex">The start index in the step text.</param>
        /// <param name="length">The length of the token in the step text.</param>
        public VariableToken(string variableName, int startIndex, int length)
            : base(startIndex, length)
        {
            VariableName = variableName;
        }

        /// <summary>
        /// Gets the name of the variable.
        /// </summary>
        public string VariableName { get; }
    }
}
