using System.Diagnostics.CodeAnalysis;
using AutoStep.Compiler;
using AutoStep.Compiler.Matching;

namespace AutoStep.Elements.Parts
{
    /// <summary>
    /// Represents the result of a call to <see cref="StepMatchingPart.ApproximateMatch(StepMatchingPart)"/>.
    /// </summary>
    [SuppressMessage("Performance", "CA1815:Override equals and operator equals on value types", Justification = "Type will never be compared.")]
    public struct StepReferenceMatchResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StepReferenceMatchResult"/> struct.
        /// </summary>
        /// <param name="length">Match length.</param>
        /// <param name="isExact">Indicates whether this is an exact match.</param>
        public StepReferenceMatchResult(int length, bool isExact)
        {
            Length = length;
            IsExact = isExact;
        }

        /// <summary>
        /// Gets the length of the match.
        /// </summary>
        public int Length { get; }

        /// <summary>
        /// Gets a value indicating whether this is an exact match.
        /// </summary>
        public bool IsExact { get; }
    }
}
