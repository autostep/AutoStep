using System.Diagnostics;
using AutoStep.Elements.Interaction;

namespace AutoStep.Language.Interaction.Traits
{
    [DebuggerDisplay("{DebuggerToString()}")]
    internal struct TraitNode
    {
        public TraitRef Ref { get; set; }

        public TraitDefinitionElement Trait { get; set; }

        public int NumberOfReferencedTraits => Ref.NumberOfReferencedTraits;

        public string DebuggerToString()
        {
            return Ref.DebuggerToString();
        }

        public bool EntirelyContainedIn(TraitNameMatchingSet traits)
        {
            if (Ref.TopLevelName is object)
            {
                return traits.Contains(Ref.TopLevelName);
            }

            return traits.Contains(Ref.ReferencedTraits);
        }
    }
}
