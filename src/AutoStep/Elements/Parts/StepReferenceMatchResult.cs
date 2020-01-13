using System;
using System.Diagnostics.CodeAnalysis;
using AutoStep.Compiler;
using AutoStep.Compiler.Matching;

namespace AutoStep.Elements.Parts
{
    /// <summary>
    /// Represents the result of a call to <see cref="StepMatchingPart.ApproximateMatch(StepMatchingPart)"/>.
    /// </summary>
    [SuppressMessage("Performance", "CA1815:Override equals and operator equals on value types", Justification = "Type will never be compared.")]
    public ref struct StepReferenceMatchResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StepReferenceMatchResult"/> struct.
        /// </summary>
        /// <param name="length">Match length.</param>
        /// <param name="isExact">Indicates whether this is an exact match.</param>
        public StepReferenceMatchResult(int length, bool isExact, ReadOnlySpan<ContentPart> remainingSpan)
        {
            Length = length;
            IsExact = isExact;
            NewSpan = remainingSpan;
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
        /// The new span on the set of content parts after this match result has happened.
        /// </summary>
        public ReadOnlySpan<ContentPart> NewSpan { get; }
    }
}
