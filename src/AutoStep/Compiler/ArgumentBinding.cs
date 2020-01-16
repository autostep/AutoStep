using System;
using System.Collections.Generic;
using System.Text;
using AutoStep.Elements.Parts;
using AutoStep.Elements.StepTokens;

namespace AutoStep.Compiler
{
    /// <summary>
    /// Represents a single argument binding to a step definition's argument, described as a range of tokens.
    /// </summary>
    public class ArgumentBinding
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ArgumentBinding"/> class.
        /// </summary>
        /// <param name="part">The definition part.</param>
        /// <param name="matchResult">The match result following a match inside the tree.</param>
        internal ArgumentBinding(ArgumentPart part, StepReferenceMatchResult matchResult)
        {
            Part = part;
            MatchedTokens = matchResult.MatchedTokens.ToArray();
            StartExclusive = matchResult.StartExclusive;
            EndExclusive = matchResult.EndExclusive;
        }

        /// <summary>
        /// Gets the definition part.
        /// </summary>
        internal ArgumentPart Part { get; }

        /// <summary>
        /// Gets the set of all matched tokens.
        /// </summary>
        internal StepToken[] MatchedTokens { get; }

        /// <summary>
        /// Gets a value indicating whether the actual value of the argument is exclusive of the first token.
        /// </summary>
        internal bool StartExclusive { get; }

        /// <summary>
        /// Gets a value indicating whether the actual value of the argument is exclusive of the last token.
        /// </summary>
        internal bool EndExclusive { get; }

        /// <summary>
        /// Gets the determined type of the argument by the linker.
        /// </summary>
        public ArgumentType? DeterminedType { get; internal set; }
    }
}
