using System;
using System.Diagnostics.CodeAnalysis;
using AutoStep.Elements.StepTokens;

namespace AutoStep.Elements.Parts
{
    /// <summary>
    /// Represents the result of a call to <see cref="DefinitionPart.DoStepReferenceMatch(string, ReadOnlySpan{StepToken})"/>.
    /// </summary>
    [SuppressMessage("Performance", "CA1815:Override equals and operator equals on value types", Justification = "Type will never be compared.")]
    internal ref struct StepReferenceMatchResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StepReferenceMatchResult"/> struct.
        /// </summary>
        /// <param name="length">Match length.</param>
        /// <param name="isExact">Indicates whether this is an exact match.</param>
        /// <param name="remainingSpan">The remaining span of step tokens after this match has been executed.</param>
        public StepReferenceMatchResult(int length, bool isExact, ReadOnlySpan<StepToken> remainingSpan, ReadOnlySpan<StepToken> matchedTokens)
        {
            Length = length;
            IsExact = isExact;
            RemainingTokens = remainingSpan;
            MatchedTokens = matchedTokens;
            StartExclusive = false;
            EndExclusive = false;
        }

        public StepReferenceMatchResult(int length, bool isExact, ReadOnlySpan<StepToken> remainingSpan, ReadOnlySpan<StepToken> matchedTokens, bool startExclusive, bool endExclusive)
            : this(length, isExact, remainingSpan, matchedTokens)
        {
            StartExclusive = startExclusive;
            EndExclusive = endExclusive;
        }

        /// <summary>
        /// Gets the length of the match.
        /// </summary>
        public int Length { get; }

        /// <summary>
        /// Gets a value indicating whether this is an exact match.
        /// </summary>
        public bool IsExact { get; }

        /// <summary>
        /// Gets the new span on the set of content parts indicating where the next definition part should start from.
        /// </summary>
        public ReadOnlySpan<StepToken> RemainingTokens { get; }

        public ReadOnlySpan<StepToken> MatchedTokens { get; }

        public bool StartExclusive { get; }

        public bool EndExclusive { get; }
    }
}
