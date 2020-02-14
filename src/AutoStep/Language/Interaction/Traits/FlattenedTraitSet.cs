using System.Collections.Generic;
using AutoStep.Elements.Interaction;
using AutoStep.Language.Interaction.Parser;

namespace AutoStep.Language.Interaction.Traits
{
    public class FlattenedTraitSet
    {
        /// <summary>
        /// The last trait is the most specific one.
        /// </summary>
        public List<TraitDefinitionElement> OrderedTraits { get; } = new List<TraitDefinitionElement>();

        public void Add(TraitDefinitionElement trait)
        {
            OrderedTraits.Add(trait);
        }
    }
}
