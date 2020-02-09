using System;
using System.Diagnostics;

namespace AutoStep.Language.Interaction.Traits
{
    [DebuggerDisplay("{DebuggerToString()}")]
    public class TraitNameMatchingSet
    {
        private ulong finishedMask;
        private string[] myTraits;

        public TraitNameMatchingSet(params string[] names)
        {
            Array.Sort(names);
            myTraits = names;

            finishedMask = (1u << names.Length) - 1;
        }

        public bool AnyLeft(ulong currentMask)
        {
            return (currentMask & finishedMask) != finishedMask;
        }

        public bool ConsumeIfContains(string name, ref ulong currentMask)
        {
            for (var idx = 0; idx < myTraits.Length; idx++)
            {
                ulong consumeFlag = 1u << idx;

                // Check if this has already been consumed (rather than do a string compare).
                if ((consumeFlag & currentMask) == 0 && myTraits[idx] == name)
                {
                    currentMask |= consumeFlag;

                    return true;
                }
            }

            return false;
        }

        public bool ConsumeIfContains(string[] sortedNames, ref ulong currentMask)
        {
            ulong foundMask = 0;
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
                    ulong thisMask = 1u << myTraitIdx;

                    if ((thisMask & currentMask) == 0 && myTraits[myTraitIdx] == sortedNames[idx])
                    {
                        // This is a match. Break so we can continue to the next one search item.
                        foundMask |= thisMask;
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
                currentMask |= foundMask;
                return true;
            }

            return false;
        }

        public string DebuggerToString()
        {
            return string.Join(" + ", myTraits);
        }
    }
}
