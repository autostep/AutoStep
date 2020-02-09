using System.Collections.Generic;

namespace AutoStep.Language.Interaction.Traits
{
    public class FlattenedTraitSet
    {
        /// <summary>
        /// The last trait is the most specific one.
        /// </summary>
        public List<Trait> OrderedTraits { get; } = new List<Trait>();

        public void Merge(FlattenedTraitSet applicableTraitSet)
        {
            OrderedTraits.AddRange(applicableTraitSet.OrderedTraits);
        }

        public void Add(Trait trait)
        {
            OrderedTraits.Add(trait);
        }
    }
}
