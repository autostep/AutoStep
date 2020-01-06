using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using AutoStep.Elements;
using AutoStep.Sources;

namespace AutoStep.Matching
{
    /// <summary>
    /// Represents a single node in the matching tree used to locate step definitions.
    /// </summary>
    internal class MatchingTreeNode
    {
        /// <summary>
        /// The left-most (first) step definition from this node downwards.
        /// </summary>
        private LinkedListNode<StepDefinition>? leftDefinition;

        /// <summary>
        /// The right-most (last) step definition from this node downwards.
        /// </summary>
        private LinkedListNode<StepDefinition>? rightDefinition;

        /// <summary>
        /// The set of definitions that are an exact match for this point in the tree.
        /// </summary>
        private LinkedList<LinkedListNode<StepDefinition>>? exactMatchNodes;

        /// <summary>
        /// The children of this node.
        /// </summary>
        private LinkedList<MatchingTreeNode>? children;

        /// <summary>
        /// Gets the set of all definitions.
        /// </summary>
        private LinkedList<StepDefinition> allDefinitions;

        /// <summary>
        /// Gets the part used for matching this node.
        /// </summary>
        private StepMatchingPart? matchingPart;

        /// <summary>
        /// Initializes a new instance of the <see cref="MatchingTreeNode"/> class.
        /// </summary>
        /// <param name="allStepDefinitions">The set of all definitions.</param>
        /// <param name="part">The matching part used by this node.</param>
        public MatchingTreeNode(LinkedList<StepDefinition> allStepDefinitions, StepMatchingPart? part)
        {
            allDefinitions = allStepDefinitions;
            matchingPart = part;
        }

        /// <summary>
        /// Add a definition to this node.
        /// </summary>
        /// <param name="definitionNode">The definition node.</param>
        /// <param name="allDefinitionParts">The set of all matching parts associated to the definition.</param>
        /// <param name="nextPartPosition">The position in allDefinitionParts that this node should look at for comparison.</param>
        public void AddDefinition(
            LinkedListNode<StepDefinition> definitionNode,
            IReadOnlyList<StepMatchingPart> allDefinitionParts,
            int nextPartPosition)
        {
            var addPosition = nextPartPosition;
            nextPartPosition++;

            if (leftDefinition is null)
            {
                // Determine the left position (starts out as the first def for the node).
                leftDefinition = definitionNode;
            }

            // Is there a part to look at?
            if (addPosition < allDefinitionParts.Count)
            {
                // Yes, so we need to check if any of our children are a match.
                var newPart = allDefinitionParts[addPosition];

                // We are not at the end of the list, that means this node needs children.
                if (children is null)
                {
                    // No, we need to create one.
                    children = new LinkedList<MatchingTreeNode>();
                }

                // Search for an equivalent match.
                var nextSearch = children.First;

                while (nextSearch is object)
                {
                    var nodeValue = nextSearch.Value;

                    if (nodeValue.matchingPart!.IsExactMatch(newPart))
                    {
                        // Found an exact match for the new part, this means that one of the children is a match.
                        nodeValue.AddDefinition(definitionNode, allDefinitionParts, nextPartPosition);
                        break;
                    }

                    nextSearch = nextSearch.Next;
                }

                if (nextSearch is null)
                {
                    // Out of things to search, looks like a new child.
                    var child = new MatchingTreeNode(allDefinitions, newPart);

                    children.AddLast(child);

                    child.AddDefinition(definitionNode, allDefinitionParts, nextPartPosition);
                }
            }
            else
            {
                if (exactMatchNodes == null)
                {
                    exactMatchNodes = new LinkedList<LinkedListNode<StepDefinition>>();
                }

                // I need to add the definition node to the linked list (it is now the furthest right-hand side of the list).
                if (rightDefinition is null)
                {
                    allDefinitions.AddLast(definitionNode);

                    // This is the last position in the list, so it must be an exact match.
                    exactMatchNodes.AddLast(definitionNode);
                }
                else
                {
                    // Check if this definition already exists (and should just be replaced).
                    LinkedListNode<StepDefinition>? currentNode = leftDefinition;
                    while (currentNode != null)
                    {
                        // This is the same definition (but possibly a new version).
                        // Replace it.
                        if (currentNode.Value.IsSameDefinition(definitionNode.Value))
                        {
                            // Just replace the node in-place.
                            currentNode.Value = definitionNode.Value;
                            break;
                        }

                        if (currentNode == rightDefinition)
                        {
                            currentNode = null;
                        }
                        else
                        {
                            currentNode = currentNode.Next;
                        }
                    }

                    // No existing node found, just add the new definition.
                    if (currentNode is null)
                    {
                        allDefinitions.AddAfter(rightDefinition, definitionNode);
                        exactMatchNodes.AddLast(definitionNode);
                    }
                }
            }

            rightDefinition = definitionNode;
        }

        /// <summary>
        /// Starts searching for matches starting from this node, treating it as a root node.
        /// </summary>
        /// <param name="results">A set to add match results to.</param>
        /// <param name="allSearchParts">The set of parts to search for.</param>
        /// <param name="exactOnly">Whether to return exact matches only.</param>
        /// <param name="partsMatched">Sets a value containing the number of parts (out of allSearchParts) that were matched during the search.</param>
        public void SearchRoot(LinkedList<MatchResult> results, IReadOnlyList<StepMatchingPart> allSearchParts, bool exactOnly, ref int partsMatched)
        {
            if (children is object)
            {
                var currentChild = children.First;
                do
                {
                    currentChild.Value.SearchMatches(results, allSearchParts, 0, exactOnly, ref partsMatched);
                    currentChild = currentChild.Next;
                }
                while (currentChild != null);
            }
        }

        /// <summary>
        /// Searches this node (and all children, if needed) for matches against the current search part.
        /// </summary>
        /// <param name="results">A set to add match results to.</param>
        /// <param name="allSearchParts">The available search parts.</param>
        /// <param name="nextSearchPartPosition">The current position in allSearchParts.</param>
        /// <param name="exactOnly">Whether to return exact matches only.</param>
        /// <param name="partsMatched">Sets a value containing the number of parts (out of allSearchParts) that were matched during the search.</param>
        /// <returns>true if this node (or any child) added results to the list.</returns>
        public bool SearchMatches(LinkedList<MatchResult> results, IReadOnlyList<StepMatchingPart> allSearchParts, int nextSearchPartPosition, bool exactOnly, ref int partsMatched)
        {
            Debug.Assert(matchingPart is object);

            // Returns true if this child (or one of it's children) has added one or more results to the list.
            var currentPart = allSearchParts[nextSearchPartPosition];
            nextSearchPartPosition++;

            // Check for match quality between the part assigned to this node and the part we are looking for.
            var match = matchingPart.ApproximateMatch(currentPart);

            if (match.Length == 0)
            {
                // No match, nothing at this node or below.
                return false;
            }
            else
            {
                var addAllRemaining = true;
                var ignoreExact = false;
                var addedSomething = false;

                // A match of some form.
                if (nextSearchPartPosition == allSearchParts.Count)
                {
                    // If this is an exact match, do we have an exact step def for it?
                    if (match.IsExact && exactMatchNodes is object && exactMatchNodes.Count > 0)
                    {
                        ignoreExact = true;

                        foreach (var exact in exactMatchNodes)
                        {
                            results.AddFirst(new MatchResult(true, int.MaxValue, exact.Value));
                        }

                        addedSomething = true;
                    }
                }
                else if (children is object)
                {
                    var currentChild = children.First;

                    do
                    {
                        var childMatched = currentChild.Value.SearchMatches(results, allSearchParts, nextSearchPartPosition, exactOnly, ref partsMatched);

                        if (childMatched)
                        {
                            // A more specific match was found, so don't use any results from higher in the tree.
                            addAllRemaining = false;
                            addedSomething = true;
                        }

                        currentChild = currentChild.Next;
                    }
                    while (currentChild != null);
                }

                if (addAllRemaining)
                {
                    if (!exactOnly)
                    {
                        // Add whatever items we have between left and right definitions.
                        var currentNode = leftDefinition;

                        // Add every node up until the right-hand side.
                        while (currentNode != null)
                        {
                            if (!ignoreExact || (exactMatchNodes is object && !exactMatchNodes.Contains(currentNode)))
                            {
                                addedSomething = true;
                                results.AddLast(new MatchResult(false, match.Length, currentNode.Value));
                            }

                            if (currentNode == rightDefinition)
                            {
                                currentNode = null;
                            }
                            else
                            {
                                currentNode = currentNode.Next;
                            }
                        }
                    }

                    partsMatched = nextSearchPartPosition;
                }

                return addedSomething;
            }
        }
    }
}
