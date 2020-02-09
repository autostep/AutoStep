using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace AutoStep.Language.Interaction.Traits
{
    [DebuggerDisplay("{DebuggerToString()}")]
    public struct TraitRef : IEquatable<TraitRef>
    {
        public TraitRef(string singleComponent)
        {
            TopLevelName = singleComponent;
            ReferencedTraits = Array.Empty<string>();
        }

        public TraitRef(string[] sortedComponents)
        {
            if (sortedComponents.Length > 1)
            {
                ReferencedTraits = sortedComponents;
                TopLevelName = null;
            }
            else
            {
                TopLevelName = sortedComponents[0];
                ReferencedTraits = Array.Empty<string>();
            }
        }

        public int NumberOfReferencedTraits => ReferencedTraits.Length;

        public string TopLevelName { get; set; }

        public string[] ReferencedTraits { get; set; }

        public string DebuggerToString()
        {
            return TopLevelName ?? string.Join(" + ", ReferencedTraits);
        }

        public override bool Equals(object obj)
        {
            return obj is TraitRef @ref && Equals(@ref);
        }

        public bool Equals([AllowNull] TraitRef other)
        {
            return TopLevelName == other.TopLevelName &&
                   EqualityComparer<string[]>.Default.Equals(ReferencedTraits, other.ReferencedTraits);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(TopLevelName, ReferencedTraits);
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
