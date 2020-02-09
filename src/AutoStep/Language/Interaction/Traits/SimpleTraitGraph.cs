using System.Collections.Generic;

namespace AutoStep.Language.Interaction.Traits
{
    /// <summary>
    /// Trait graph behaviour
    ///     - Add all traits to a sorted set of traits, ordered by number of referenced traits.    
    ///     - ComputeDependences - Starting at the trait with the most dependencies:
    ///         - Look at the traits it depends on.
    ///         - Find the next trait down the list where all the traits of that parent are in the dependency list of the current item.
    ///         - Mark those dependencies as processed in the child.
    ///         - Recurse for that found parent.
    ///         - When recursion has exited, we add the 
    ///         
    ///     - SearchDependencies.
    ///         - Takes a set of traits
    /// 
    /// </summary>
    public class SimpleTraitGraph
    {
        public SimpleTraitGraph()
        {
        }

        public LinkedList<TraitNode> AllTraits { get; } = new LinkedList<TraitNode>();

        public Dictionary<TraitRef, Trait> TraitLookup { get; set; } = new Dictionary<TraitRef, Trait>();

        public void AddOrExtendTrait(Trait newTrait)
        {
            var newRef = newTrait.GetRef();

            if (TraitLookup.TryGetValue(newRef, out var trait))
            {
                // This is just an update to an existing trait.
                trait.ExtendWith(newTrait);

                return;
            }

            // Add a new trait to the sorted set of traits.
            TraitLookup.Add(newRef, newTrait);

            var newNode = new TraitNode 
            { 
                Ref = newRef,
                Trait = newTrait
            };

            // Find the insertion point.
            var existingNode = AllTraits.First;

            if (existingNode is null)
            {
                AllTraits.AddFirst(newNode);
            }
            else 
            {
                // Go through the set until this new node is less complex than the current node in
                // the list.
                while (newNode.NumberOfReferencedTraits < existingNode.Value.NumberOfReferencedTraits)
                {
                    var nextNode = existingNode.Next;

                    if (nextNode is null)
                    {
                        break;
                    }
                    else
                    {
                        existingNode = existingNode.Next;
                    }
                }

                // Add before the existing node.
                AllTraits.AddBefore(existingNode, newNode);
            }
        }

        public FlattenedTraitSet MatchTraits(string[] namedTraits)
        {
            var applicableTraitSet = new FlattenedTraitSet();

            // Start from the beginning of the set.
            var currentItem = AllTraits.First;

            // Define a set of traits that can say they've already been consumed.
            var traitMatchingSet = new TraitNameMatchingSet(namedTraits);

            // Start at the end.
            while (currentItem != null)
            {
                var mask = 0ul;
                if (currentItem.Value.ConsumeIfEntirelyContainedIn(traitMatchingSet, ref mask))
                {
                    applicableTraitSet.Add(currentItem.Value.Trait);
                }

                currentItem = currentItem.Next;
            }

            return applicableTraitSet;
        }
    }
}
