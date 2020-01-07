using System.Collections.Generic;
using AutoStep.Definitions;
using AutoStep.Elements;

namespace AutoStep.Compiler.Matching
{
    /// <summary>
    /// Defines a tree used to search for step definitions from step references.
    /// </summary>
    public interface IMatchingTree
    {
        /// <summary>
        /// Add a definition to the tree. An existing step definition where <see cref="StepDefinition.IsSameDefinition(StepDefinition)"/>
        /// is true for the same declaration will be replaced.
        /// </summary>
        /// <param name="definition">The step definition.</param>
        void AddOrUpdateDefinition(StepDefinition definition);

        /// <summary>
        /// Executes a matching search for step definitions that match the specified step reference.
        /// </summary>
        /// <param name="stepReference">A step reference to match against.</param>
        /// <param name="exactOnly">If true, only exact matches are returned (slightly faster for failing searches).</param>
        /// <param name="partsMatched">The number of parts of the step references matching parts that were used to match (i.e. how long in the search process did the reference stay valid).</param>
        /// <returns>The match results.</returns>
        LinkedList<MatchResult> Match(StepReferenceElement stepReference, bool exactOnly, out int partsMatched);

        void RemoveDefinition(StepDefinition def);
    }
}
