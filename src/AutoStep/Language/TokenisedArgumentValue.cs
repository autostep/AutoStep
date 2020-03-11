using AutoStep.Elements.StepTokens;

namespace AutoStep.Language
{
    /// <summary>
    /// Represents a set of matched tokens inside a piece of text.
    /// </summary>
    public class TokenisedArgumentValue
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TokenisedArgumentValue"/> class.
        /// </summary>
        /// <param name="matchedTokens">The set of matched tokens.</param>
        /// <param name="startExclusive">Indicates whether the actual value of the argument is exclusive of the first token.</param>
        /// <param name="endExclusive">Indicates whether the actual value of the argument is exclusive of the klast token.</param>
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
        public bool StartExclusive { get; }

        /// <summary>
        /// Gets a value indicating whether the actual value of the argument is exclusive of the last token.
        /// </summary>
        public bool EndExclusive { get; }
    }
}
