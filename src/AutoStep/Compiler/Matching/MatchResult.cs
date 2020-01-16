using System;
using System.Collections.Generic;
using AutoStep.Definitions;
using AutoStep.Elements.Parts;
using AutoStep.Elements.StepTokens;

namespace AutoStep.Compiler.Matching
{
    /// <summary>
    /// Defines a match result found while searching the MatchingTree.
    /// </summary>
    internal class MatchResult
    {
        private List<CompilerMessage>? msgs;

        /// <summary>
        /// Initializes a new instance of the <see cref="MatchResult"/> class.
        /// </summary>
        /// <param name="isExact">Whether this is an exact match.</param>
        /// <param name="confidence">The confidence level.</param>
        /// <param name="definition">The matched definition.</param>
        public MatchResult(bool isExact, int confidence, StepDefinition definition)
        {
            IsExact = isExact;
            Confidence = confidence;
            Definition = definition;
            msgs = null;
            ArgumentSet = null;
        }

        /// <summary>
        /// Gets the confidence rating for the match. A higher confidence indicates that the match
        /// is more likely to be right for the provided input.
        /// </summary>
        public int Confidence { get; }

        /// <summary>
        /// Gets the matched definition.
        /// </summary>
        public StepDefinition Definition { get; }

        /// <summary>
        /// Gets a value indicating whether this is an exact one for the definition.
        /// </summary>
        public bool IsExact { get; }

        /// <summary>
        /// Gets the set of argument bindings for this result.
        /// </summary>
        public LinkedList<ArgumentBinding>? ArgumentSet { get; private set; }

        /// <summary>
        /// Adds a compiler message to the match result.
        /// </summary>
        /// <param name="msg">The compilation message.</param>
        public void AddMessage(CompilerMessage msg)
        {
            if (msgs is null)
            {
                msgs = new List<CompilerMessage>();
            }

            msgs.Add(msg);
        }

        public void PrependArgumentSet(ArgumentPart arg, StepReferenceMatchResult matchResult)
        {
            if (ArgumentSet is null)
            {
                ArgumentSet = new LinkedList<ArgumentBinding>();
            }

            ArgumentSet.AddFirst(new ArgumentBinding(arg, matchResult));
        }
    }
}
