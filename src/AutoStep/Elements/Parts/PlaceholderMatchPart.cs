using System;
using System.Collections.Generic;
using AutoStep.Elements.StepTokens;

namespace AutoStep.Elements.Parts
{
    /// <summary>
    /// Represents a defined step argument.
    /// </summary>
    internal class PlaceholderMatchPart : DefinitionPart
    {
        private List<WordDefinitionPart> matchingPlaceholderValues = new List<WordDefinitionPart>();

        public string PlaceholderValueName { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PlaceholderMatchPart"/> class.
        /// </summary>
        /// <param name="placeholderValueName">The name of the placeholder value.</param>
        public PlaceholderMatchPart(string placeholderValueName)
            : base(string.Empty)
        {
            PlaceholderValueName = placeholderValueName;
        }

        public void MatchValue(string placeholderValue)
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
                else if (match.Length > bestResult.Length)
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

        /// <summary>
        /// Returns the content of the matched text, given the reference text.
        /// </summary>
        /// <param name="referenceText">The entire reference text.</param>
        /// <param name="matchedTokens">The set of matched tokens.</param>
        /// <returns>The placeholder content.</returns>
        public string GetContent(string referenceText, ReadOnlySpan<StepToken> matchedTokens)
        {
            // Content of a placeholder will just be the literal text between the start and end.
            if (matchedTokens.Length == 0)
            {
                return string.Empty;
            }
            else if (matchedTokens.Length == 1)
            {
                var matched = matchedTokens[0];
                return referenceText.Substring(matched.StartIndex, matched.Length);
            }
            else
            {
                var start = matchedTokens[0].StartIndex;
                var lastToken = matchedTokens[matchedTokens.Length - 1];
                var length = (lastToken.StartIndex - start) + lastToken.Length;

                return referenceText.Substring(start, length);
            }
        }
    }
}
