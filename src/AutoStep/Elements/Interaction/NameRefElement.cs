namespace AutoStep.Elements.Interaction
{
    /// <summary>
    /// Represents a name reference in an interaction file.
    /// </summary>
    public class NameRefElement : PositionalElement
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NameRefElement"/> class.
        /// </summary>
        /// <param name="name">The name value.</param>
        public NameRefElement(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Gets the name value.
        /// </summary>
        public string Name { get; }
    }
}
