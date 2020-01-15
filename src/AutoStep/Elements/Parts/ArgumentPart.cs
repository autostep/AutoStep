using System;
using AutoStep.Compiler;
using AutoStep.Elements.StepTokens;

namespace AutoStep.Elements.Parts
{
    internal class ArgumentPart : DefinitionPart
    {
        public string? Name { get; set; }

        public ArgumentType? TypeHint { get; set; }

        public override StepReferenceMatchResult DoStepReferenceMatch(string referenceText, ReadOnlySpan<StepToken> referenceParts)
        {
            var currentPart = referenceParts[0];
            var remainingParts = referenceParts.Slice(1);
            var stopOnGap = true;
            var matchedPartsCount = 1;
            var contentPartsIndex = 0;
            var contentPartsLength = 1;

            QuoteToken? startQuote = currentPart as QuoteToken;

            if (startQuote is QuoteToken)
            {
                // Starting on a quote, so continue past gaps.
                stopOnGap = false;
                contentPartsIndex = 1;
                contentPartsLength = 0;

                // If this part is the last available part, then we have to assume that the resulting part is
                // just the quote character being passed as an argument. Bit odd, but it's fine.
                if (remainingParts.IsEmpty)
                {
                    return new StepReferenceMatchResult(matchedPartsCount, true, remainingParts, referenceParts.Slice(0, 1));
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
                    break;
                }
                else
                {
                    contentPartsLength++;
                    remainingParts = remainingParts.Slice(1);
                }
            }

            return new StepReferenceMatchResult(matchedPartsCount, true, remainingParts, referenceParts.Slice(contentPartsIndex, contentPartsLength));
        }

        public CompilerMessage? GetBindingMessage(ReadOnlySpan<StepToken> referenceParts)
        {
            // Ok, so, does a step reference match? A step reference will 'match' any other part, but then we are going to apply some extra
            // logic and see whether the value fits.
            if (referenceParts[0] is VariableToken var)
            {
                // It's a match, a variable can be anything (late-bound), so we will say it matches.

            }
            else if (referenceParts[0] is WordToken word)
            {
                // Text will match, but let's look at the type of the argument.

            }

            // TODO
            return null;
        }

        public override bool IsDefinitionPartMatch(DefinitionPart part)
        {
            return part is ArgumentPart otherArg &&
                   Name == otherArg.Name &&
                   TypeHint == otherArg.TypeHint;
        }
    }

}
