using System;
using System.Collections.Generic;
using AutoStep.Elements.Interaction;

namespace AutoStep.Language.Interaction.Traits
{
    /// <summary>
    /// Represents a graph of the set of all traits available. Provides functions to walk the set of traits, matching on trait names.
    /// </summary>
    /// <remarks>
    /// The trait graph is composed of a sorted list of traits, organised by complexity (most complex first).
    ///
    /// Traits are identified by a 'trait reference', implemented by <see cref="TraitRef"/>. This structure
    /// gives a 'signature' to a trait, based on the trait combination.
    ///
    /// Considering a set of traits A, B, C, D and E, then an example trait graph might look like the following list,
    /// where each item is a single trait reference.
    ///
    ///    A + B + C + D + E
    ///    A + B + C
    ///    C + D + E
    ///    A + B
    ///    C + D
    ///    A
    ///    B
    ///    C
    ///    D
    ///    E
    ///
    /// The reason for the ordering is because more complex traits take precedence when building method tables,
    /// and it allows us to bail early when searching the set of traits.
    ///
    /// A trait lookup dictionary allows us to do a fast check for existing traits with the same reference.
    /// </remarks>
    public class TraitGraph
    {
        private readonly LinkedList<TraitNode> allTraits = new LinkedList<TraitNode>();
        private readonly Dictionary<TraitRef, TraitDefinitionElement> traitLookup = new Dictionary<TraitRef, TraitDefinitionElement>();

        /// <summary>
        /// Gets the set of all traits.
        /// </summary>
        public IReadOnlyCollection<TraitDefinitionElement> AllTraits => traitLookup.Values;

        /// <summary>
        /// Add a trait to the trait graph, or extend an existing one with the same <see cref="TraitRef"/>.
        /// </summary>
        /// <param name="newTrait">The new trait element to add. If an existing one is found with the same reference, then it will be merged.</param>
        public void AddOrExtendTrait(TraitDefinitionElement newTrait)
        {
            if (newTrait is null)
            {
                throw new ArgumentNullException(nameof(newTrait));
            }

            // Get the trait reference for this trait.
            var newRef = newTrait.GetRef();

            if (traitLookup.TryGetValue(newRef, out var trait))
            {
                // This is just an update to an existing trait.
                trait.ExtendWith(newTrait);

                return;
            }

            // Add the new trait to our lookup.
            traitLookup.Add(newRef, newTrait);

            // Define a new node to insert into our ordered linked list.
            var newNode = new TraitNode(newRef, newTrait);

            // Find the insertion point.
            var existingNode = allTraits.First;

            if (existingNode is null)
            {
                // No existing node, this is the first item.
                allTraits.AddFirst(newNode);
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

                    existingNode = existingNode.Next;
                }

                // Add before the existing node.
                allTraits.AddBefore(existingNode, newNode);
            }
        }

        /// <summary>
        /// Merge a given <see cref="TraitGraph" /> into this one.
        /// </summary>
        /// <param name="graph">The graph to merge.</param>
        public void Merge(TraitGraph graph)
        {
            graph = graph.ThrowIfNull(nameof(graph));

            // Both graphs will already be sorted in the same order, so we can just go through both and merge them.
            var currentGraphNode = allTraits.First;
            var mergingGraphNode = graph.allTraits.First;

            // While we have nodes left to merge in.
            while (mergingGraphNode is object)
            {
                // The 'number of referenced traits' is our complexity value. So until the merging node is less complicated than our current graph's node,
                // we'll move through our set.
                if (currentGraphNode is object && mergingGraphNode.Value.NumberOfReferencedTraits < currentGraphNode.Value.NumberOfReferencedTraits)
                {
                    currentGraphNode = currentGraphNode.Next;
                }
                else
                {
                    // Ok, we need to add in this node, first we get its reference.
                    var newRef = mergingGraphNode.Value.Ref;

                    if (currentGraphNode is null)
                    {
                        // We have no graph nodes left, add it to the end.
                        traitLookup.Add(newRef, mergingGraphNode.Value.Trait);
                        allTraits.AddLast(mergingGraphNode.Value);
                    }
                    else if (traitLookup.TryGetValue(newRef, out var trait))
                    {
                        // This is just an update to an existing trait.
                        trait.ExtendWith(mergingGraphNode.Value.Trait);
                    }
                    else
                    {
                        // Add before the existing node (to preserve complexity order).
                        allTraits.AddBefore(currentGraphNode, mergingGraphNode.Value);
                    }

                    mergingGraphNode = mergingGraphNode.Next;
                }
            }
        }

        /// <summary>
        /// Search the set of traits, invoking a callback for each trait that overlaps with the set of named traits provided.
        /// The trait set is walked in order from simplest to most complex.
        /// </summary>
        /// <typeparam name="TContext">The context object type.</typeparam>
        /// <param name="namedTraits">The set of trait names.</param>
        /// <param name="context">A context object to pass to the callback.</param>
        /// <param name="callback">The callback to invoke.</param>
        /// <remarks>
        /// A trait will match if all of its defined traits (e.g. A + B), exist in the provided set of named traits (e.g. A, B, C).
        /// </remarks>
        public void SearchTraits<TContext>(IEnumerable<string> namedTraits, TContext context, Action<TContext, TraitDefinitionElement> callback)
        {
            if (namedTraits is null)
            {
                throw new ArgumentNullException(nameof(namedTraits));
            }

            if (callback is null)
            {
                throw new ArgumentNullException(nameof(callback));
            }

            // Start from the end of the set (simplest).
            var currentItem = allTraits.Last;

            var traitMatchingSet = new TraitNameMatchingSet(namedTraits);

            // Go through the set of traits simplest first. We can bail early this way, by stopping when the complexity
            // of the traits we're looking at is higher than the provided set.
            while (currentItem != null && currentItem.Value.NumberOfReferencedTraits <= traitMatchingSet.Count)
            {
                if (currentItem.Value.EntirelyContainedIn(traitMatchingSet))
                {
                    // Trait matches.
                    callback(context, currentItem.Value.Trait);
                }

                currentItem = currentItem.Previous;
            }
        }

        /// <summary>
        /// Walks the entire trait graph, invoking the provided callback when the complete method table for a given
        /// trait is known.
        /// Traits are walked most-complex to least-complex.
        /// </summary>
        /// <param name="rootTable">The root method table to start from.</param>
        /// <param name="traitVisitCallback">The callback to invoke.</param>
        public void MethodTableWalk(MethodTable rootTable, Action<TraitDefinitionElement, MethodTable> traitVisitCallback)
        {
            // Ok, so starting from the most complex, work to the most simple.
            var currentTrait = allTraits.First;

            while (currentTrait is object)
            {
                // For each node, start at the simplest, searching back down towards this one.
                var matchingSet = new TraitNameMatchingSet(currentTrait.Value.Ref);
                var currentSearchTrait = allTraits.Last;
                var methodTable = new MethodTable(rootTable);

                // Walk back down until we reach this one.
                while (currentSearchTrait != currentTrait)
                {
                    // The current search trait is entirely contained in our search set.
                    if (currentSearchTrait.Value.EntirelyContainedIn(matchingSet))
                    {
                        foreach (var traitMethod in currentSearchTrait.Value.Trait.Methods)
                        {
                            // Merge each method in.
                            methodTable.Set(traitMethod.Key, traitMethod.Value);
                        }
                    }

                    currentSearchTrait = currentSearchTrait.Previous;
                }

                foreach (var traitMethod in currentSearchTrait.Value.Trait.Methods)
                {
                    // Merge any methods in the current trait.
                    methodTable.Set(traitMethod.Key, traitMethod.Value);
                }

                // We now have the complete method table. Invoke the callback that can do any validation it needs to.
                traitVisitCallback(currentSearchTrait.Value.Trait, methodTable);

                currentTrait = currentTrait.Next;
            }
        }
    }
}
