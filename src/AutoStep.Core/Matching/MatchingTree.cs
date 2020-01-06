using System;
using System.Collections.Generic;
using System.Text;
using AutoStep.Core.Elements;
using AutoStep.Core.Sources;

namespace AutoStep.Core.Matching
{
    public class MatchingTree : IMatchingTree
    {
        private LinkedList<StepDefinition> allGivenDefinitions;
        private MatchingTreeNode rootGivenNode;
        private LinkedList<StepDefinition> allWhenDefinitions;
        private MatchingTreeNode rootWhenNode;
        private LinkedList<StepDefinition> allThenDefinitions;
        private MatchingTreeNode rootThenNode;

        public MatchingTree()
        {
            allGivenDefinitions = new LinkedList<StepDefinition>();
            rootGivenNode = new MatchingTreeNode(allGivenDefinitions, null);

            allWhenDefinitions = new LinkedList<StepDefinition>();
            rootWhenNode = new MatchingTreeNode(allWhenDefinitions, null);

            allThenDefinitions = new LinkedList<StepDefinition>();
            rootThenNode = new MatchingTreeNode(allThenDefinitions, null);
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

            var root = definition.Type switch
            {
                StepType.Given => rootGivenNode,
                StepType.When => rootWhenNode,
                StepType.Then => rootThenNode,
                _ => throw new ArgumentException("Invalid step definition binding type.", nameof(definition))
            };

            var defNode = new LinkedListNode<StepDefinition>(definition);
            root.AddDefinition(defNode, allParts, 0);
        }

        public LinkedList<(int confidence, StepDefinition def)> Match(StepReferenceElement stepReference, out int partsMatched)
        {
            if (stepReference is null)
            {
                throw new ArgumentNullException(nameof(stepReference));
            }

            if (!stepReference.BindingType.HasValue)
            {
                throw new ArgumentException("Step reference must have a known binding type.", nameof(stepReference));
            }

            var list = new LinkedList<(int confidence, StepDefinition def)>();
            partsMatched = 0;

            var root = stepReference.BindingType.Value switch
            {
                StepType.Given => rootGivenNode,
                StepType.When => rootWhenNode,
                StepType.Then => rootThenNode,
                _ => throw new ArgumentException("Invalid step definition binding type.")
            };

            root.SearchRoot(list, stepReference.MatchingParts, 0, ref partsMatched);

            return list;
        }
    }
}
