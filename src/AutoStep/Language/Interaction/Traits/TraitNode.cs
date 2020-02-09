using System.Diagnostics;

namespace AutoStep.Language.Interaction.Traits
{
    [DebuggerDisplay("{DebuggerToString()}")]
    public struct TraitNode
    {
        public TraitRef Ref { get; set; }

        public Trait Trait { get; set; }

        public bool HaveDeterminedParents => ApplicableTraitSet is object;

        public int NumberOfReferencedTraits => Ref.NumberOfReferencedTraits;

        public string DebuggerToString()
        {
            return Ref.DebuggerToString();
        }

        public FlattenedTraitSet ApplicableTraitSet { get; set; }

        public bool ConsumeIfEntirelyContainedIn(TraitNameMatchingSet traits, ref ulong consumedMask)
        {
            if (Ref.TopLevelName is object)
            {
                // If this method returns true, the set will have consumed that 
                return traits.ConsumeIfContains(Ref.TopLevelName, ref consumedMask);
            }

            return traits.ConsumeIfContains(Ref.ReferencedTraits, ref consumedMask);
        }
    }
}
