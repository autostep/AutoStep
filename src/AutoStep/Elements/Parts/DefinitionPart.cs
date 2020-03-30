using System;
using AutoStep.Elements.StepTokens;

namespace AutoStep.Elements.Parts
{
    /// <summary>
    /// Represents the base part of a step definition that has text and the ability to compare against step references.
    /// </summary>
    public abstract class DefinitionPart : PositionalElement
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DefinitionPart"/> class.
        /// </summary>
        /// <param name="text">The text content of the part.</param>
        protected DefinitionPart(string text)
        {
            Text = text;
        }

        /// <summary>
        /// Gets the text content of the part.
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// Checks whether another part is the 'same' as this one from the perspective of matching. This is different from an equals comparison.
        /// </summary>
        /// <param name="part">The part to test against.</param>
        /// <returns>True if the part is the same.</returns>
        public abstract bool IsDefinitionPartMatch(DefinitionPart part);

        /// <summary>
        /// Executes a step reference match, seeing if the remaining tokens in the step match this part.
        /// </summary>
        /// <param name="referenceText">The step text.</param>
        /// <param name="remainingTokenSpan">The remaining tokens.</param>
        /// <returns>A match result.</returns>
        internal abstract StepReferenceMatchResult DoStepReferenceMatch(string referenceText, ReadOnlySpan<StepToken> remainingTokenSpan);
    }
}
