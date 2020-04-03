using System.Collections.Generic;

namespace AutoStep.Elements.Interaction
{
    /// <summary>
    /// Represents an item in an interaction definition file that can have methods and steps (i.e. components and traits).
    /// </summary>
    public abstract class InteractionDefinitionElement : BuiltElement
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InteractionDefinitionElement"/> class.
        /// </summary>
        /// <param name="id">The element ID.</param>
        public InteractionDefinitionElement(string id)
        {
            Id = Name = id;
        }

        /// <summary>
        /// Gets or sets the ID of the definition.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the definition.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the source name.
        /// </summary>
        public string? SourceName { get; set; }

        /// <summary>
        /// Gets the set of methods declared on the definition.
        /// </summary>
        public Dictionary<string, MethodDefinitionElement> Methods { get; } = new Dictionary<string, MethodDefinitionElement>();

        /// <summary>
        /// Gets the set of declared steps.
        /// </summary>
        public List<InteractionStepDefinitionElement> Steps { get; } = new List<InteractionStepDefinitionElement>();
    }
}
