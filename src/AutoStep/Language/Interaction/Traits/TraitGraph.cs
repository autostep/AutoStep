using System;
using System.Collections.Generic;
using AutoStep.Elements.Interaction;

namespace AutoStep.Language.Interaction.Traits
{
    /// <summary>
    /// Trait graph behaviour.
    /// </summary>
    public class TraitGraph
    {
        public TraitGraph()
        {
        }

        internal LinkedList<TraitNode> AllTraits { get; } = new LinkedList<TraitNode>();

        internal Dictionary<TraitRef, TraitDefinitionElement> TraitLookup { get; set; } = new Dictionary<TraitRef, TraitDefinitionElement>();

        public void AddOrExtendTrait(TraitDefinitionElement newTrait)
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
                Trait = newTrait,
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

        public void Merge(TraitGraph graph)
        {
            graph = graph.ThrowIfNull(nameof(graph));

            // Both graphs are sorted in the same order.
            var currentGraphNode = AllTraits.First;
            var mergingGraphNode = graph.AllTraits.First;

            while (mergingGraphNode is object)
            {
                if (currentGraphNode is object && mergingGraphNode.Value.NumberOfReferencedTraits < currentGraphNode.Value.NumberOfReferencedTraits)
                {
                    currentGraphNode = currentGraphNode.Next;
                }
                else
                {
                    var newRef = mergingGraphNode.Value.Ref;

                    if (currentGraphNode is null)
                    {
                        TraitLookup.Add(newRef, mergingGraphNode.Value.Trait);
                        AllTraits.AddLast(mergingGraphNode.Value);
                    }
                    else if (TraitLookup.TryGetValue(newRef, out var trait))
                    {
                        // This is just an update to an existing trait.
                        trait.ExtendWith(mergingGraphNode.Value.Trait);
                    }
                    else
                    {
                        // Add before the existing node.
                        AllTraits.AddBefore(currentGraphNode, mergingGraphNode.Value);
                    }

                    mergingGraphNode = mergingGraphNode.Next;
                }
            }
        }

        public FlattenedTraitSet MatchTraits(NameRefElement[] namedTraits)
        {
            var applicableTraitSet = new FlattenedTraitSet();

            SearchTraits(namedTraits, applicableTraitSet, (s, t) => s.Add(t));

            return applicableTraitSet;
        }

        public void SearchTraits<TContext>(NameRefElement[] namedTraits, TContext context, Action<TContext, TraitDefinitionElement> callback)
        {
            var numberOfTraits = namedTraits.Length;

            // Start from the beginning of the set.
            var currentItem = AllTraits.First;

            var traitMatchingSet = new TraitNameMatchingSet(namedTraits);

            while (currentItem != null && currentItem.Value.NumberOfReferencedTraits <= numberOfTraits)
            {
                if (currentItem.Value.EntirelyContainedIn(traitMatchingSet))
                {
                    callback(context, currentItem.Value.Trait);
                }

                currentItem = currentItem.Next;
            }
        }

        public void SearchTraitsSimplestFirst<TContext>(NameRefElement[] namedTraits, TContext context, Action<TContext, TraitDefinitionElement> callback)
        {
            var numberOfTraits = namedTraits.Length;

            // Start from the end of the set (simplest).
            var currentItem = AllTraits.Last;

            var traitMatchingSet = new TraitNameMatchingSet(namedTraits);

            while (currentItem != null && currentItem.Value.NumberOfReferencedTraits <= numberOfTraits)
            {
                if (currentItem.Value.EntirelyContainedIn(traitMatchingSet))
                {
                    callback(context, currentItem.Value.Trait);
                }

                currentItem = currentItem.Previous;
            }
        }
    }
}
