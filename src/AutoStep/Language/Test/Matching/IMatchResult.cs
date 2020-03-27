using System;
using System.Collections.Generic;
using System.Text;
using AutoStep.Definitions;

namespace AutoStep.Language.Test.Matching
{
    /// <summary>
    /// Represents a match result from a step definition search.
    /// </summary>
    public interface IMatchResult
    {
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
        public IEnumerable<ArgumentBinding> Arguments { get; }

        /// <summary>
        /// Gets the set of matched placeholder values for the match result. Will be null if there are no placeholders.
        /// </summary>
        public IReadOnlyDictionary<string, string>? PlaceholderValues { get; }
    }
}
