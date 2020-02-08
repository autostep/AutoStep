using System;
using System.Collections.Generic;
using System.Text;
using AutoStep.Elements.Parts;
using AutoStep.Elements.StepTokens;

namespace AutoStep.Language.Test
{

    /// <summary>
    /// Represents a single argument binding to a step definition's argument, described as a range of tokens.
    /// </summary>
    public class ArgumentBinding : TokenisedArgumentValue
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ArgumentBinding"/> class.
        /// </summary>
        /// <param name="part">The definition part.</param>
        /// <param name="matchResult">The match result following a match inside the tree.</param>
        internal ArgumentBinding(ArgumentPart part, StepReferenceMatchResult matchResult)
            : base(matchResult.MatchedTokens.ToArray(), matchResult.StartExclusive, matchResult.EndExclusive)
        {
            Part = part;
        }

        /// <summary>
        /// Gets the definition part.
        /// </summary>
        internal ArgumentPart Part { get; }

        /// <summary>
        /// Gets the determined type of the argument by the linker.
        /// </summary>
        public ArgumentType? DeterminedType { get; internal set; }
    }
}
