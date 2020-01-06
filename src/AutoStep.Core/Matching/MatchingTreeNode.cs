using System;
using System.Collections.Generic;
using System.Text;
using AutoStep.Core.Elements;
using AutoStep.Core.Sources;

namespace AutoStep.Core.Matching
{

    internal class MatchingTreeNode
    {
        private LinkedListNode<StepDefinition> leftDefinition;
        private LinkedListNode<StepDefinition> rightDefinition;
        private LinkedList<LinkedListNode<StepDefinition>> exactMatchNodes;
        private LinkedList<MatchingTreeNode> children;

        public MatchingTreeNode(LinkedList<StepDefinition> allStepDefinitions, StepMatchingPart part)
        {
            AllDefinitions = allStepDefinitions;
            Part = part;
            exactMatchNodes = new LinkedList<LinkedListNode<StepDefinition>>();
        }

        public LinkedList<StepDefinition> AllDefinitions { get; }

        public StepMatchingPart Part { get; }

        public void AddDefinition(LinkedListNode<StepDefinition> definitionNode,
                                  IReadOnlyList<StepMatchingPart> allDefinitionParts, int nextPartPosition)
        {
            var addPosition = nextPartPosition;
            nextPartPosition++;

            if (leftDefinition is null)
            {
                leftDefinition = definitionNode;
            }

            if (addPosition < allDefinitionParts.Count)
            {
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

                    if (nodeValue.Part.IsExactMatch(newPart))
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
                    var child = new MatchingTreeNode(AllDefinitions, newPart);

                    children.AddLast(child);

                    child.AddDefinition(definitionNode, allDefinitionParts, nextPartPosition);
                }
            }
            else
            {
                // If this is a leaf node, then I need to add the definition node to the linked list (it is now the furthest right-hand side of the list).
                if (rightDefinition is null)
                {
                    AllDefinitions.AddLast(definitionNode);
                }
                else
                {
                    AllDefinitions.AddAfter(rightDefinition, definitionNode);
                }

                // This is the last position in the list, so it must be an exact match.
                // TODO: Go through and check for duplicates/replacements?
                exactMatchNodes.AddLast(definitionNode);
            }

            rightDefinition = definitionNode;
        }

        public bool SearchRoot(LinkedList<(int confidence, StepDefinition def)> results, IReadOnlyList<StepMatchingPart> allSearchParts, int nextSearchPartPosition, ref int partsMatched)
        {
            if (children is null)
            {
                return false;
            }
            else
            {
                var currentChild = children.First;
                var anyChildMatched = false;
                do
                {
                    var childMatched = currentChild.Value.SearchMatches(results, allSearchParts, nextSearchPartPosition, ref partsMatched);

                    if (childMatched)
                    {
                        return true;
                    }

                    currentChild = currentChild.Next;
                }
                while (currentChild != null);

                return anyChildMatched;
            }
        }

        public bool SearchMatches(LinkedList<(int confidence, StepDefinition def)> results, IReadOnlyList<StepMatchingPart> allSearchParts, int nextSearchPartPosition, ref int partsMatched)
        {
            // Returns true if this child (or one of it's children) has added one or more results to the list.
            var currentPart = allSearchParts[nextSearchPartPosition];
            nextSearchPartPosition++;

            var match = Part.ApproximateMatch(currentPart);

            if (match.length == 0)
            {
                // No match, not going to work.
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
                    // This is an exact match, do we have a step def for it?
                    if (match.isExact && exactMatchNodes.Count > 0)
                    {
                        ignoreExact = true;

                        foreach (var exact in exactMatchNodes)
                        {
                            results.AddFirst((int.MaxValue, exact.Value));
                        }

                        addedSomething = true;
                    }
                }
                else if (children is object)
                {
                    var currentChild = children.First;

                    do
                    {
                        var childMatched = currentChild.Value.SearchMatches(results, allSearchParts, nextSearchPartPosition, ref partsMatched);

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
                    // We're out of parts.
                    // Add whatever we have.
                    var currentNode = leftDefinition;

                    // Add every node up until the right-hand side.
                    while (currentNode != null)
                    {
                        if (!ignoreExact || !exactMatchNodes.Contains(currentNode))
                        {
                            addedSomething = true;
                            results.AddLast((match.length, currentNode.Value));
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

                    partsMatched = nextSearchPartPosition;
                }

                return addedSomething;
            }
        }
    }
}
