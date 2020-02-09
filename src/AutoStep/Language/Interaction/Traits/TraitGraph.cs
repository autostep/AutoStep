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
    public class TraitGraph
    {
        private readonly bool disableCaching;

        public TraitGraph()
        {
        }

        public TraitGraph(bool disableCaching)
        {
            this.disableCaching = disableCaching;
        }

        public LinkedList<TraitNode> AllTraits { get; } = new LinkedList<TraitNode>();

        public Dictionary<TraitRef, Trait> TraitLookup { get; set; } = new Dictionary<TraitRef, Trait>();

        public void AddOrExtendTrait(Trait newTrait)
        {
            var newRef = newTrait.GetRef();

            if(TraitLookup.TryGetValue(newRef, out var trait))
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

            VisitNode(currentItem, traitMatchingSet, 0, new HashSet<TraitRef>(), applicableTraitSet);

            return applicableTraitSet;
        }

        private void VisitNode(LinkedListNode<TraitNode> currentNode, TraitNameMatchingSet matchingSet, ulong traitConsumedMask, HashSet<TraitRef> visited, FlattenedTraitSet traitResultSet)
        {
            var originalNodeMask = traitConsumedMask;

            while (matchingSet.AnyLeft(traitConsumedMask) && currentNode is object)
            {
                // Find the next node in the list that is entirely consumed.
                var traitNode = currentNode.Value;
                traitConsumedMask = originalNodeMask;

                // If the set of references for the current set item is fully contained by the test ref...
                if (!visited.Contains(traitNode.Ref) && traitNode.ConsumeIfEntirelyContainedIn(matchingSet, ref traitConsumedMask))
                {
                    // Everything in this item is applicable to the set of traits.
                    // Consume it.
                    visited.Add(traitNode.Ref);

                    if (traitNode.NumberOfReferencedTraits > 0)
                    {
                        if (!disableCaching && traitNode.HaveDeterminedParents)
                        {
                            // We know the parents of this node already, so just append all parents to 
                            // the list.
                            traitResultSet.Merge(traitNode.ApplicableTraitSet);
                        }
                        else
                        {
                            // We have not visited this tree item before, visit it.
                            // Create a new flattened trait set.
                            traitNode.ApplicableTraitSet = new FlattenedTraitSet();

                            // The trait mask for pursuing this path is only those things already consumed by this 
                            // path. So everything else should be marked as consumed.
                            var nextStageMask = ulong.MaxValue ^ traitConsumedMask ^ originalNodeMask;

                            VisitNode(currentNode.Next, matchingSet, nextStageMask, visited, traitNode.ApplicableTraitSet);

                            traitResultSet.Merge(traitNode.ApplicableTraitSet);
                        }
                    }

                    // Add self.
                    traitResultSet.Add(traitNode.Trait);
                }
                
                // Not a match, try the next one in the set.
                currentNode = currentNode.Next;                
            }
        }
    }
}
