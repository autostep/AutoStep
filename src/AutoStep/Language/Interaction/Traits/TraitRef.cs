using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AutoStep.Elements.Interaction;

namespace AutoStep.Language.Interaction.Traits
{
    /// <summary>
    /// Defines a trait reference, that defines a 'signature' for a trait.
    /// </summary>
    [DebuggerDisplay("{DebuggerToString()}")]
    internal struct TraitRef : IEquatable<TraitRef>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TraitRef"/> struct for a single name.
        /// </summary>
        /// <param name="singleName">The single name.</param>
        public TraitRef(string singleName)
        {
            TopLevelName = singleName;
            ReferencedTraits = Array.Empty<string>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TraitRef"/> struct, for a multiple-name trait.
        /// </summary>
        /// <param name="sortedNames">A set of pre-sorted names.</param>
        public TraitRef(IReadOnlyList<string> sortedNames)
        {
            if (sortedNames.Count > 1)
            {
                ReferencedTraits = sortedNames;
                TopLevelName = null;
            }
            else
            {
                TopLevelName = sortedNames[0];
                ReferencedTraits = Array.Empty<string>();
            }
        }

        /// <summary>
        /// Gets the number of referenced traits. Singular-named traits have 0 referenced traits, so the minimum
        /// number will be 2.
        /// </summary>
        public int NumberOfReferencedTraits => ReferencedTraits.Count;

        /// <summary>
        /// Gets the top-level name for this trait. Will be null if this is a multi-named trait.
        /// </summary>
        public string? TopLevelName { get; }

        /// <summary>
        /// Gets the set of reference traits. Will be empty if this is a single-named trait.
        /// </summary>
        public IReadOnlyList<string> ReferencedTraits { get; }

        public static bool operator ==(TraitRef left, TraitRef right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(TraitRef left, TraitRef right)
        {
            return !(left == right);
        }

        /// <summary>
        /// Gets debugger content.
        /// </summary>
        /// <returns>The debugger text.</returns>
        public string DebuggerToString()
        {
            return TopLevelName ?? string.Join(" + ", ReferencedTraits);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return obj is TraitRef @ref && Equals(@ref);
        }

        /// <inheritdoc/>
        public bool Equals(TraitRef other)
        {
            return TopLevelName == other.TopLevelName
                    && ReferencedTraits.SequenceEqual(other.ReferencedTraits);
        }

        /// <inheritdoc/>
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
    }
}
