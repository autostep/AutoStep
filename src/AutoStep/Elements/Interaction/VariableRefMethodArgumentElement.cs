namespace AutoStep.Elements.Interaction
{
    /// <summary>
    /// Defines a variable reference element.
    /// </summary>
    public class VariableRefMethodArgumentElement : MethodArgumentElement
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VariableRefMethodArgumentElement"/> class.
        /// </summary>
        /// <param name="variableName">The name of the variable.</param>
        public VariableRefMethodArgumentElement(string variableName)
        {
            VariableName = variableName;
        }

        /// <summary>
        /// Gets the name of the variable.
        /// </summary>
        public string VariableName { get; }
    }
}
