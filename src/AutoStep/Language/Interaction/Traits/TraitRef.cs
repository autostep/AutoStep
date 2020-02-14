using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AutoStep.Elements.Interaction;

namespace AutoStep.Language.Interaction.Traits
{
    [DebuggerDisplay("{DebuggerToString()}")]
    internal struct TraitRef : IEquatable<TraitRef>
    {
        public TraitRef(NameRefElement singleComponent)
        {
            TopLevelName = singleComponent;
            ReferencedTraits = Array.Empty<NameRefElement>();
        }

        public TraitRef(NameRefElement[] sortedComponents)
        {
            if (sortedComponents.Length > 1)
            {
                ReferencedTraits = sortedComponents;
                TopLevelName = null;
            }
            else
            {
                TopLevelName = sortedComponents[0];
                ReferencedTraits = Array.Empty<NameRefElement>();
            }
        }

        public int NumberOfReferencedTraits => ReferencedTraits.Length;

        public NameRefElement TopLevelName { get; set; }

        public NameRefElement[] ReferencedTraits { get; private set; }

        public string DebuggerToString()
        {
            return TopLevelName?.Name ?? string.Join(" + ", ReferencedTraits.Select(n => n.Name));
        }

        public override bool Equals(object obj)
        {
            return obj is TraitRef @ref && Equals(@ref);
        }

        public bool Equals([AllowNull] TraitRef other)
        {
            return TopLevelName == other.TopLevelName &&
                   ReferencedTraits.SequenceEqual(other.ReferencedTraits);
        }

        public override int GetHashCode()
        {
            HashCode myHash = default;
            myHash.Add(TopLevelName);

            foreach (var trait in ReferencedTraits)
            {
                myHash.Add(trait);
            }

            return myHash.ToHashCode();
        }

        public static bool operator ==(TraitRef left, TraitRef right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(TraitRef left, TraitRef right)
        {
            return !(left == right);
        }
    }
}
