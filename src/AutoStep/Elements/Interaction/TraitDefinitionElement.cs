using System;
using System.Collections.Generic;
using System.Linq;
using AutoStep.Language.Interaction.Traits;

namespace AutoStep.Elements.Interaction
{
    /// <summary>
    /// Represents an interaction trait definition.
    /// </summary>
    public class TraitDefinitionElement : InteractionDefinitionElement
    {
        private IReadOnlyList<string> textNames = Array.Empty<string>();

        /// <summary>
        /// Initializes a new instance of the <see cref="TraitDefinitionElement"/> class.
        /// </summary>
        /// <param name="id">The full text ID of the trait.</param>
        public TraitDefinitionElement(string id)
            : base(id)
        {
        }

        /// <summary>
        /// Provide the set of name parts for the trait.
        /// </summary>
        /// <param name="nameParts">The set of parsed names.</param>
        public void ProvideNameParts(IReadOnlyList<NameRefElement> nameParts)
        {
            if (nameParts is null)
            {
                throw new ArgumentNullException(nameof(nameParts));
            }

            if (nameParts.Count == 0)
            {
                throw new ArgumentException(ElementExceptionMessages.TraitsMustHaveAtLeastOneNamePart, nameof(nameParts));
            }

            // Sort the components so each identical trait (i.e. same combination of parent traits) has exactly the same
            // value.
            textNames = nameParts.Select(x => x.Name).OrderBy(x => x).ToList();
            NameElements = nameParts;
        }

        /// <summary>
        /// Gets the number of referenced traits.
        /// </summary>
        public int NumberOfReferencedTraits => NameElements?.Count > 1 ? NameElements.Count : 0;

        /// <summary>
        /// Gets the name elements.
        /// </summary>
        public IReadOnlyList<NameRefElement>? NameElements { get; private set; }

        /// <summary>
        /// Get a trait reference for this trait (effectively the signature for a trait).
        /// </summary>
        /// <returns>The trait reference.</returns>
        internal TraitRef GetRef()
        {
            return new TraitRef(textNames);
        }

        /// <summary>
        /// Extend this trait definition with other.
        /// </summary>
        /// <param name="newTrait">The new trait to merge in.</param>
        internal void ExtendWith(TraitDefinitionElement newTrait)
        {
            foreach (var item in newTrait.Methods)
            {
                // Override the method (or add it if it doesn't exist).
                Methods[item.Key] = item.Value;
            }

            Steps.AddRange(newTrait.Steps);
        }
    }
}
