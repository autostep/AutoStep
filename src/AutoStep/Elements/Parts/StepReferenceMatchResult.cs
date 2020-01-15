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
        public StepReferenceMatchResult(int length, bool isExact, ReadOnlySpan<StepToken> remainingSpan, ReadOnlySpan<StepToken> resultParts = default)
        {
            Length = length;
            IsExact = isExact;
            RemainingTokens = remainingSpan;
            ResultTokens = resultParts;
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

        /// <summary>
        /// Gets a span containing the 'result' of the match (if relevant), that will be available to <see cref="ArgumentPart"/> parts 
        /// when returning up the tree after an exact match.
        /// </summary>
        public ReadOnlySpan<StepToken> ResultTokens { get; }
    }
}
