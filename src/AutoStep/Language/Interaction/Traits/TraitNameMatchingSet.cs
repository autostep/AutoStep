using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AutoStep.Elements.Interaction;

namespace AutoStep.Language.Interaction.Traits
{
    /// <summary>
    /// Represents a set of names used for testing against traits during <see cref="TraitGraph"/> searches.
    /// </summary>
    [DebuggerDisplay("{DebuggerToString()}")]
    internal struct TraitNameMatchingSet
    {
        private readonly IReadOnlyList<string> myTraits;

        /// <summary>
        /// Initializes a new instance of the <see cref="TraitNameMatchingSet"/> struct.
        /// </summary>
        /// <param name="names">The set of names.</param>
        public TraitNameMatchingSet(IEnumerable<string> names)
        {
            myTraits = names.OrderBy(x => x).ToList();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TraitNameMatchingSet"/> struct from an existing known trait reference.
        /// </summary>
        /// <param name="tRef">The trait reference.</param>
        public TraitNameMatchingSet(TraitRef tRef)
        {
            // We know that trait references are pre-sorted, so just re-use it.
            myTraits = tRef.ReferencedTraits;
        }

        /// <summary>
        /// Gets the number of traits represented by this set.
        /// </summary>
        public int Count => myTraits.Count;

        /// <summary>
        /// Checks if the trait set contains a single named trait reference.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>True if the set contains the requested name; false otherwise.</returns>
        public bool Contains(string name)
        {
            for (var idx = 0; idx < myTraits.Count; idx++)
            {
                if (myTraits[idx] == name)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Test if this trait name matching set fully contains the specified sorted elements. An unsorted input will result in unspecified behaviour.
        /// </summary>
        /// <param name="sortedNames">A pre-sorted set of names.</param>
        /// <returns>True if the set matches, false otherwise.</returns>
        public bool Contains(IReadOnlyList<string> sortedNames)
        {
            sortedNames = sortedNames.ThrowIfNull(nameof(sortedNames));

            var myTraitIdx = 0;
            var foundCount = 0;

            // For each name, check if that name exists in the set (and has not been consumed).
            for (var idx = 0; idx < sortedNames.Count; idx++)
            {
                // Because everything is sorted, the next value in the sortedNames
                // list is going to come after the last match in the myTraits. More efficient than searching from the beginning
                // each time.
                while (myTraitIdx < myTraits.Count)
                {
                    if (myTraits[myTraitIdx] == sortedNames[idx])
                    {
                        foundCount++;
                        myTraitIdx++;
                        break;
                    }

                    myTraitIdx++;
                }

                if (myTraitIdx == myTraits.Count)
                {
                    // We've gone beyond the end, so not a match.
                    break;
                }
            }

            if (foundCount == sortedNames.Count)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Get debugger output.
        /// </summary>
        /// <returns>The debugger display content.</returns>
        public string DebuggerToString()
        {
            return string.Join(" + ", myTraits);
        }
    }
}
