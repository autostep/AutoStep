using System;
using AutoStep.Compiler;
using AutoStep.Elements.StepTokens;

namespace AutoStep.Elements.Parts
{
    /// <summary>
    /// Represents a defined step argument.
    /// </summary>
    internal class ArgumentPart : DefinitionPart
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ArgumentPart"/> class.
        /// </summary>
        public ArgumentPart()
            : base(string.Empty)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArgumentPart"/> class.
        /// </summary>
        /// <param name="text">The argument definition text.</param>
        /// <param name="variableName">The variable name.</param>
        /// <param name="typeHint">A type hint for the parameter (if we have one).</param>
        public ArgumentPart(string text, string variableName, ArgumentType? typeHint = null)
            : base(text)
        {
            if (text is null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            Name = variableName ?? throw new ArgumentNullException(nameof(variableName));
            TypeHint = typeHint;
        }

        /// <summary>
        /// Gets the variable name, if we know it.
        /// </summary>
        public string? Name { get; }

        /// <summary>
        /// Gets the type hint for the argument, if we know it.
        /// </summary>
        public ArgumentType? TypeHint { get; }

        /// <inheritdoc/>
        public override StepReferenceMatchResult DoStepReferenceMatch(string referenceText, ReadOnlySpan<StepToken> referenceParts)
        {
            var firstPart = referenceParts[0];
            var currentPart = firstPart;
            var remainingParts = referenceParts.Slice(1);
            var stopOnGap = true;
            var matchedPartsCount = 1;
            var contentPartsLength = 1;
            var startExclusive = false;
            var endExclusive = false;

            QuoteToken? startQuote = currentPart as QuoteToken;

            if (startQuote is QuoteToken)
            {
                // Starting on a quote, so continue past gaps.
                stopOnGap = false;

                // If this part is the last available part, then we have to assume that the resulting part is
                // just the quote character being passed as an argument. Bit odd, but it's fine.
                if (remainingParts.IsEmpty)
                {
                    return new StepReferenceMatchResult(matchedPartsCount, true, remainingParts, referenceParts.Slice(0, 1));
                }
                else
                {
                    startExclusive = true;
                }
            }

            while (!remainingParts.IsEmpty)
            {
                var lastPart = currentPart;
                currentPart = remainingParts[0];

                // First, figure out if there's a gap.
                if (currentPart.StartIndex > (lastPart.StartIndex + lastPart.Length))
                {
                    // The new part does not start immediately after the previous one,
                    // indicating that there is a gap. If we are stopping on a gap, then
                    // end here.
                    if (stopOnGap)
                    {
                        break;
                    }
                }

                matchedPartsCount++;

                if (currentPart is QuoteToken quote &&
                    startQuote is object &&
                    startQuote.IsDoubleQuote == quote.IsDoubleQuote)
                {
                    // The quote is the same type; that means we have hit the end of the quoted section, and the
                    // rest of this argument.
                    remainingParts = remainingParts.Slice(1);
                    endExclusive = true;
                    break;
                }
                else
                {
                    contentPartsLength++;
                    remainingParts = remainingParts.Slice(1);
                }
            }

            return new StepReferenceMatchResult(matchedPartsCount, true, remainingParts, referenceParts.Slice(0, matchedPartsCount), startExclusive, endExclusive);
        }

        /// <inheritdoc/>
        public override bool IsDefinitionPartMatch(DefinitionPart part)
        {
            return part is ArgumentPart otherArg &&
                   Name == otherArg.Name &&
                   TypeHint == otherArg.TypeHint;
        }
    }
}
