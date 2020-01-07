using System;
using System.Collections.Generic;
using System.Text;
using AutoStep.Definitions;
using AutoStep.Elements;

namespace AutoStep.Compiler.Matching
{
    /// <summary>
    /// Implements the matching tree.
    /// </summary>
    internal class MatchingTree : IMatchingTree
    {
        private LinkedList<StepDefinition> allGivenDefinitions;
        private MatchingTreeNode rootGivenNode;
        private LinkedList<StepDefinition> allWhenDefinitions;
        private MatchingTreeNode rootWhenNode;
        private LinkedList<StepDefinition> allThenDefinitions;
        private MatchingTreeNode rootThenNode;

        /// <summary>
        /// Initializes a new instance of the <see cref="MatchingTree"/> class.
        /// </summary>
        public MatchingTree()
        {
            allGivenDefinitions = new LinkedList<StepDefinition>();
            rootGivenNode = new MatchingTreeNode(allGivenDefinitions, null);

            allWhenDefinitions = new LinkedList<StepDefinition>();
            rootWhenNode = new MatchingTreeNode(allWhenDefinitions, null);

            allThenDefinitions = new LinkedList<StepDefinition>();
            rootThenNode = new MatchingTreeNode(allThenDefinitions, null);
        }

        /// <summary>
        /// Add a definition to the tree. An existing step definition where <see cref="StepDefinition.IsSameDefinition(StepDefinition)"/>
        /// is true for the same declaration will be replaced.
        /// </summary>
        /// <param name="definition">The step definition.</param>
        public void AddOrUpdateDefinition(StepDefinition definition)
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

            var root = GetRootNodeForDefinition(definition);

            var defNode = new LinkedListNode<StepDefinition>(definition);
            root.AddDefinition(defNode, allParts, 0);
        }

        public void RemoveDefinition(StepDefinition definition)
        {
            if (definition is null)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            if (definition.Definition is null)
            {
                // Can't remove a definition without metadata.
                return;
            }

            var allParts = definition.Definition.MatchingParts;

            if (allParts.Count == 0)
            {
                // Can't add a definition with no matching parts, so can't remove it either.
                return;
            }

            var root = GetRootNodeForDefinition(definition);

            root.RemoveDefinition(definition, allParts, 0);
        }

        /// <summary>
        /// Executes a matching search for step definitions that match the specified step reference.
        /// </summary>
        /// <param name="stepReference">A step reference to match against.</param>
        /// <param name="exactOnly">If true, only exact matches are returned (slightly faster for failing searches).</param>
        /// <param name="partsMatched">The number of parts of the step references matching parts that were used to match (i.e. how long in the search process did the reference stay valid).</param>
        /// <returns>The match results.</returns>
        public LinkedList<MatchResult> Match(StepReferenceElement stepReference, bool exactOnly, out int partsMatched)
        {
            if (stepReference is null)
            {
                throw new ArgumentNullException(nameof(stepReference));
            }

            if (!stepReference.BindingType.HasValue)
            {
                throw new ArgumentException(MatchingTreeMessages.StepReferenceMustHaveKnownBindingType, nameof(stepReference));
            }

            var list = new LinkedList<MatchResult>();
            partsMatched = 0;

            var root = stepReference.BindingType.Value switch
            {
                StepType.Given => rootGivenNode,
                StepType.When => rootWhenNode,
                StepType.Then => rootThenNode,
                _ => throw new ArgumentException(MatchingTreeMessages.InvalidStepReferenceBindingType, nameof(stepReference))
            };

            root.SearchRoot(list, stepReference.MatchingParts, exactOnly, ref partsMatched);

            return list;
        }

        private MatchingTreeNode GetRootNodeForDefinition(StepDefinition definition)
        {
            return definition.Type switch
            {
                StepType.Given => rootGivenNode,
                StepType.When => rootWhenNode,
                StepType.Then => rootThenNode,
                _ => throw new ArgumentException(MatchingTreeMessages.InvalidStepDefinitionBindingType, nameof(definition))
            };
        }
    }
}
