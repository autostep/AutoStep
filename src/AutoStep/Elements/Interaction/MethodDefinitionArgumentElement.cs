namespace AutoStep.Elements.Interaction
{
    /// <summary>
    /// Defines a named method definition argument.
    /// </summary>
    public class MethodDefinitionArgumentElement : PositionalElement
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MethodDefinitionArgumentElement"/> class.
        /// </summary>
        /// <param name="name">The argument name.</param>
        public MethodDefinitionArgumentElement(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Gets the argument name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets or sets the type hint for a parameter.
        /// </summary>
        public string? TypeHint { get; set; }
    }

}
