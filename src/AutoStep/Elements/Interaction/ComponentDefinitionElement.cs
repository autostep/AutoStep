using System.Collections.Generic;

namespace AutoStep.Elements.Interaction
{
    /// <summary>
    /// Represents a built component definition.
    /// </summary>
    public class ComponentDefinitionElement : InteractionDefinitionElement
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ComponentDefinitionElement"/> class.
        /// </summary>
        /// <param name="id">The component ID.</param>
        public ComponentDefinitionElement(string id)
            : base(id)
        {
        }

        /// <summary>
        /// Gets the set of traits assigned to the component.
        /// </summary>
        public List<NameRefElement> Traits { get; } = new List<NameRefElement>();

        /// <summary>
        /// Gets or sets the 'inherits' property of the component.
        /// </summary>
        public NameRefElement? Inherits { get; set; }
    }
}
