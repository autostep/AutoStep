using System;
using System.Collections.Generic;
using AutoStep.Definitions;

namespace AutoStep.Compiler.Matching
{
    /// <summary>
    /// Defines a match result found while searching the MatchingTree.
    /// </summary>
    internal struct MatchResult : IEquatable<MatchResult>
    {
        private List<CompilerMessage>? msgs;

        /// <summary>
        /// Initializes a new instance of the <see cref="MatchResult"/> struct.
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

        public IReadOnlyList<CompilerMessage> Messages => msgs;

        /// <summary>
        /// Gets a value indicating whether this is an exact one for the definition.
        /// </summary>
        public bool IsExact { get; }

        /// <summary>
        /// Equals comparison.
        /// </summary>
        /// <param name="left">Left.</param>
        /// <param name="right">Right.</param>
        /// <returns>Equivalent.</returns>
        public static bool operator ==(MatchResult left, MatchResult right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// !Equals comparison.
        /// </summary>
        /// <param name="left">Left.</param>
        /// <param name="right">Right.</param>
        /// <returns>!Equivalent.</returns>
        public static bool operator !=(MatchResult left, MatchResult right)
        {
            return !(left == right);
        }


        public void AddMessage(CompilerMessage msg)
        {
            msgs.Add(msg);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (obj is MatchResult m)
            {
                return Equals(m);
            }

            return false;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return HashCode.Combine(Confidence, Definition, IsExact);
        }

        /// <inheritdoc/>
        public bool Equals(MatchResult other)
        {
            return other.Confidence == Confidence &&
                   other.Definition == Definition &&
                   other.IsExact == IsExact;
        }
    }
}
