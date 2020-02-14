using System;
using AutoStep.Elements.StepTokens;

namespace AutoStep.Elements.Parts
{
    /// <summary>
    /// Represents a defined step argument.
    /// </summary>
    internal class ComponentMatchPart : DefinitionPart
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ArgumentPart"/> class.
        /// </summary>
        public ComponentMatchPart()
            : base(string.Empty)
        {
        }

        /// <inheritdoc/>
        public override StepReferenceMatchResult DoStepReferenceMatch(string referenceText, ReadOnlySpan<StepToken> referenceParts)
        {
            var currentPart = referenceParts[0];

            if (currentPart is TextToken)
            {
                var refSpan = referenceText.AsSpan(currentPart.StartIndex, currentPart.Length);

                // TODO: Need to figure out how to define the list of components for a particular step definition.
                if (refSpan.Equals("button"))
                {
                    return new StepReferenceMatchResult(1, true, referenceParts.Slice(1), referenceParts.Slice(0, 1));
                }
            }

            return new StepReferenceMatchResult(0, false, referenceParts, ReadOnlySpan<StepToken>.Empty);
        }

        /// <inheritdoc/>
        public override bool IsDefinitionPartMatch(DefinitionPart part)
        {
            return part is ComponentMatchPart;
        }
    }
}
