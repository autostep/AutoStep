using System;
using System.Diagnostics;
using System.Linq;
using AutoStep.Elements.Interaction;

namespace AutoStep.Language.Interaction.Traits
{
    [DebuggerDisplay("{DebuggerToString()}")]
    internal struct TraitNameMatchingSet
    {
        private NameRefElement[] myTraits;

        public TraitNameMatchingSet(params NameRefElement[] names)
        {
            Array.Sort(names);
            myTraits = names;
        }

        public TraitNameMatchingSet(TraitRef tRef)
        {
            myTraits = tRef.ReferencedTraits;
        }

        public bool Contains(NameRefElement name)
        {
            for (var idx = 0; idx < myTraits.Length; idx++)
            {
                if (myTraits[idx].Name == name.Name)
                {
                    return true;
                }
            }

            return false;
        }

        public bool Contains(NameRefElement[] sortedNames)
        {
            sortedNames = sortedNames.ThrowIfNull(nameof(sortedNames));

            var myTraitIdx = 0;
            var foundCount = 0;

            // For each name, check if that name exists in the set (and has not been consumed).
            for (var idx = 0; idx < sortedNames.Length; idx++)
            {
                // Because everything is sorted, the next value in the sortedNames
                // list is going to come after the last match in the myTraits. More efficient than searching from the beginning
                // each time.
                while (myTraitIdx < myTraits.Length)
                {
                    if (myTraits[myTraitIdx].Name == sortedNames[idx].Name)
                    {
                        foundCount++;
                        myTraitIdx++;
                        break;
                    }

                    myTraitIdx++;
                }

                if (myTraitIdx == myTraits.Length)
                {
                    // We've gone beyond the end, so not a match.
                    break;
                }
            }

            if (foundCount == sortedNames.Length)
            {
                return true;
            }

            return false;
        }

        public string DebuggerToString()
        {
            return string.Join(" + ", myTraits.Select(x => x.Name));
        }
    }
}
