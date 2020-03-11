namespace AutoStep.Elements.Interaction
{
    /// <summary>
    /// Defines an array variable reference.
    /// </summary>
    public class VariableArrayRefMethodArgument : MethodArgumentElement
    {
        /// <summary>
        /// Gets or sets the variable name.
        /// </summary>
        public string? VariableName { get; set; }

        /// <summary>
        /// Gets or sets the indexer argument.
        /// </summary>
        public MethodArgumentElement? Indexer { get; set; }
    }
}
