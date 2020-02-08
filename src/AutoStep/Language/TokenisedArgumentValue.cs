using System;
using System.Collections.Generic;
using System.Text;
using AutoStep.Elements.Parts;
using AutoStep.Elements.StepTokens;

namespace AutoStep.Language
{
    public class TokenisedArgumentValue
    {
        internal TokenisedArgumentValue(StepToken[] matchedTokens, bool startExclusive, bool endExclusive)
        {
            MatchedTokens = matchedTokens;
            StartExclusive = startExclusive;
            EndExclusive = endExclusive;
        }

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
    }
}
