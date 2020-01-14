using System;
using System.Collections.Generic;
using System.Diagnostics;
using AutoStep.Compiler;
using AutoStep.Compiler.Matching;
using AutoStep.Definitions;

namespace AutoStep.Elements.Parts
{

    internal class WordDefinitionPart : DefinitionContentPart
    {
        public string EscapedText { get; set; }

        public override StepReferenceMatchResult DoStepReferenceMatch(string referenceText, ReadOnlySpan<ContentPart> currentPartSpan)
        {
            // The word definition part should:
            // - Consume text from the current part as much as it can (until either it has consumed all the text in the part or this word has run out of content)
            // - Return a match with the span after we've done our bit.
            var defTextSpan = Text.AsSpan();
            var currentPart = currentPartSpan[0];
            var refTextSpan = referenceText.AsSpan(currentPart.StartIndex, currentPart.Length);

            int matchedLength = 0;

            // While we have some text left in this part.
            while (!defTextSpan.IsEmpty)
            {
                // This word definition is bigger than the matching text then we may need to consume
                // multiple parts.
                if (defTextSpan.Length > refTextSpan.Length)
                {
                    if (defTextSpan.StartsWith(refTextSpan, StringComparison.CurrentCulture))
                    {
                        matchedLength += refTextSpan.Length;

                        // The refTextSpan starts with the reference text, so it's been matched.
                        // Move the defTextSpan along and move to the next part.
                        defTextSpan = defTextSpan.Slice(refTextSpan.Length);

                        // Move to the next part.
                        currentPartSpan = currentPartSpan.Slice(1);

                        if (currentPartSpan.IsEmpty)
                        {
                            // Out of reference parts; suggests a partial match on this part.
                            return new StepReferenceMatchResult(matchedLength, false, currentPartSpan);
                        }
                        else
                        {
                            currentPart = currentPartSpan[0];
                            refTextSpan = referenceText.AsSpan(currentPart.StartIndex, currentPart.Length);
                        }
                    }
                    else
                    {
                        // Not a match.
                        return new StepReferenceMatchResult(matchedLength, false, currentPartSpan);
                    }
                }
                else
                {
                    var searchedCharacters = 0;
                    while (searchedCharacters < refTextSpan.Length && refTextSpan[searchedCharacters].Equals(defTextSpan[searchedCharacters]))
                    {
                        searchedCharacters++;
                    }

                    // This tells us how many characters matched.
                    matchedLength += searchedCharacters;
                    // Move the part span along.
                    currentPartSpan = currentPartSpan.Slice(1);

                    return new StepReferenceMatchResult(matchedLength, defTextSpan.Length == searchedCharacters, currentPartSpan);
                }
            }

            return new StepReferenceMatchResult(matchedLength, false, currentPartSpan);
        }

        public override bool IsDefinitionPartMatch(DefinitionContentPart part)
        {
            return part is DefinitionContentPart wrd && wrd.Text == Text;
        }

    }

}
