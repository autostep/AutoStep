using System;
using System.Collections.Generic;
using System.Text;
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

                    if ((newPart.IsArgument && newPart.ArgumentType == nodeValue.Part.ArgumentType) ||
                        (newPart.TextContent == nodeValue.Part.TextContent))
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
    }
}
