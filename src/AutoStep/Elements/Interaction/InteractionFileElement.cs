using System.Collections.Generic;
using AutoStep.Language.Interaction.Traits;

namespace AutoStep.Elements.Interaction
{
    /// <summary>
    /// Represents the top-level of an interaction file.
    /// </summary>
    public class InteractionFileElement : BuiltElement
    {
        /// <summary>
        /// Gets or sets the name of the source (usually a file name).
        /// </summary>
        public string? SourceName { get; set; }

        /// <summary>
        /// Gets the trait graph for the file.
        /// </summary>
        public TraitGraph TraitGraph { get; } = new TraitGraph();

        /// <summary>
        /// Gets the set of components defined in the file.
        /// </summary>
        public List<ComponentDefinitionElement> Components { get; } = new List<ComponentDefinitionElement>();
    }
}
