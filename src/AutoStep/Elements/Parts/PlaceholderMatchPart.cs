using System;
using System.Collections.Generic;
using AutoStep.Elements.StepTokens;

namespace AutoStep.Elements.Parts
{
    /// <summary>
    /// Represents a placeholder part, that can match a dynamic value. This allows a single step definition to match against
    /// multiple steps based on these values, and output the matched value as a result of the binding operation.
    /// </summary>
    internal class PlaceholderMatchPart : DefinitionPart
    {
        private readonly List<WordDefinitionPart> matchingPlaceholderValues = new List<WordDefinitionPart>();

        /// <summary>
        /// Initializes a new instance of the <see cref="PlaceholderMatchPart"/> class.
        /// </summary>
        /// <param name="placeholderValueName">The name of the placeholder value.</param>
        public PlaceholderMatchPart(string placeholderValueName)
            : base(string.Empty)
        {
            PlaceholderValueName = placeholderValueName;
        }

        /// <summary>
        /// Gets the name of the placeholder.
        /// </summary>
        public string PlaceholderValueName { get; }

        /// <summary>
        /// Add a value to match against.
        /// </summary>
        /// <param name="placeholderValue">The value to match.</param>
        public void AddMatchingValue(string placeholderValue)
        {
            // Add a word definition part.
            matchingPlaceholderValues.Add(new WordDefinitionPart(placeholderValue, true)
            {
                SourceLine = SourceLine,
                StartColumn = StartColumn,
                EndLine = EndLine,
                EndColumn = EndColumn,
            });
        }

        /// <summary>
        /// Reset the list of matching placeholder values.
        /// </summary>
        public void ClearAllValues()
        {
            matchingPlaceholderValues.Clear();
        }

        /// <inheritdoc/>
        public override StepReferenceMatchResult DoStepReferenceMatch(string referenceText, ReadOnlySpan<StepToken> referenceParts)
        {
            var bestResult = new StepReferenceMatchResult(0, false, referenceParts, ReadOnlySpan<StepToken>.Empty);

            // Go through the step of matching components and use the first exact match.
            // If no exact match then use the one that matches the most characters.
            foreach (var component in matchingPlaceholderValues)
            {
                var match = component.DoStepReferenceMatch(referenceText, referenceParts);

                if (match.IsExact)
                {
                    // Take an exact match immediately.
                    bestResult = match;
                    break;
                }

                if (match.Length > bestResult.Length)
                {
                    // Take the better match.
                    bestResult = match;
                }
            }

            return bestResult;
        }

        /// <inheritdoc/>
        public override bool IsDefinitionPartMatch(DefinitionPart part)
        {
            // A placeholder part matches.
            return part is PlaceholderMatchPart;
        }
    }
}
