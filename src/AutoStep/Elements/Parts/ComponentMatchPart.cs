using System;
using System.Collections.Generic;
using AutoStep.Elements.StepTokens;

namespace AutoStep.Elements.Parts
{
    /// <summary>
    /// Represents a defined step argument.
    /// </summary>
    internal class ComponentMatchPart : DefinitionPart
    {
        private List<WordDefinitionPart> matchingComponents = new List<WordDefinitionPart>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ComponentMatchPart"/> class.
        /// </summary>
        public ComponentMatchPart()
            : base(string.Empty)
        {
        }

        public void MatchComponentName(string componentName)
        {
            // Add a word definition part.
            matchingComponents.Add(new WordDefinitionPart(componentName)
            {
                SourceLine = SourceLine,
                StartColumn = StartColumn,
                EndLine = EndLine,
                EndColumn = EndColumn,
            });
        }

        public void ClearAllMatchingComponents()
        {
            matchingComponents.Clear();
        }

        /// <inheritdoc/>
        public override StepReferenceMatchResult DoStepReferenceMatch(string referenceText, ReadOnlySpan<StepToken> referenceParts)
        {
            var bestResult = new StepReferenceMatchResult(0, false, referenceParts, ReadOnlySpan<StepToken>.Empty);

            // Go through the step of matching components and use the first exact match.
            // If no exact match then use the one that matches the most characters.
            foreach (var component in matchingComponents)
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
            // A definition part matches.
            return part is ComponentMatchPart;
        }
    }
}
