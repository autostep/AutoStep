using System;
using System.Collections.Generic;
using System.Text;
using AutoStep.Core.Elements;
using AutoStep.Core.Sources;

namespace AutoStep.Core.Matching
{
    public class MatchingTree
    {
        private LinkedList<StepDefinition> allDefinitions;
        private MatchingTreeNode rootNode;

        public MatchingTree()
        {
            allDefinitions = new LinkedList<StepDefinition>();
            rootNode = new MatchingTreeNode(allDefinitions, null);
        }

        public void AddDefinition(StepDefinition definition)
        {
            if (definition is null)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            if (definition.Definition is null)
            {
                // Can't add it without metadata.
                return;
            }

            var allParts = definition.Definition.MatchingParts;

            if (allParts.Count == 0)
            {
                // Can't add a definition with no matching parts.
                return;
            }

            var defNode = new LinkedListNode<StepDefinition>(definition);
            rootNode.AddDefinition(defNode, allParts, 0);
        }

        public LinkedList<(int confidence, StepDefinition def)> Match(StepReferenceElement stepReference, out int partsMatched)
        {
            if (stepReference is null)
            {
                throw new ArgumentNullException(nameof(stepReference));
            }

            var list = new LinkedList<(int confidence, StepDefinition def)>();
            partsMatched = 0;

            rootNode.SearchRoot(list, stepReference.MatchingParts, 0, ref partsMatched);

            return list;
        }
    }

    internal class MatchingTreeNode
    {
        private LinkedListNode<StepDefinition> leftDefinition;
        private LinkedListNode<StepDefinition> rightDefinition;
        private LinkedListNode<StepDefinition> exactMatch;
        private LinkedList<MatchingTreeNode> children;

        public MatchingTreeNode(LinkedList<StepDefinition> allStepDefinitions, StepMatchingPart part)
        {
            AllDefinitions = allStepDefinitions;
            Part = part;
        }

        public LinkedList<StepDefinition> AllDefinitions { get; }

        public StepMatchingPart Part { get; }

        public void AddDefinition(LinkedListNode<StepDefinition> definitionNode, IReadOnlyList<StepMatchingPart> allDefinitionParts, int nextPartPosition)
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
            else if (exactMatch is object)
            {
                // There's already an exact match with this part? Error?
                throw new Exception();
            }
            else
            {
                // If this is a leaf node, then I need to add the definition node to the linked list (it is now the furthest right-hand side of the list).
                if (children is null)
                {
                    if (rightDefinition is null)
                    {
                        AllDefinitions.AddLast(definitionNode);
                    }
                    else
                    {
                        AllDefinitions.AddAfter(rightDefinition, definitionNode);
                    }
                }

                // This is the last position in the list, so it must be an exact match.
                exactMatch = definitionNode;
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
                }
                while (currentChild != null && currentChild != children.Last);

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
                    if (match.isExact && exactMatch is object)
                    {
                        ignoreExact = true;
                        results.AddFirst((int.MaxValue, exactMatch.Value));
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
                        if (!ignoreExact || currentNode != exactMatch)
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

                    partsMatched = allSearchParts.Count;
                }

                return addedSomething;
            }
        }
    }
}
