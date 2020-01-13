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

        public override StepReferenceMatchResult DoStepReferenceMatch(ContentPart referencePart)
        {
            throw new NotImplementedException();
            var textForDefCompare = EscapedText ?? Text;
            var textForRefCompare = string.Empty;

            // If the other one is a word, then use the escaped text from that for matching.
            if (referencePart is WordPart word && word.EscapedText != null)
            {
                textForRefCompare = word.EscapedText;
            }

            if (textForDefCompare == null || textForRefCompare == null)
            {
                Trace.Assert(false, "Parser rules should prevent null text.");
            }

            if (textForDefCompare!.Length == textForRefCompare!.Length)
            {
                // Exact length match. Do a straight-forward compare.
                if (textForDefCompare == textForRefCompare)
                {
                    // Exact match.
                    return new StepReferenceMatchResult(textForRefCompare.Length, true);
                }
                else
                {
                    return new StepReferenceMatchResult(0, false);
                }
            }
            else if (textForRefCompare.Length > textForDefCompare.Length)
            {
                // Not possible to match, the search target contains more text than this part.
                return new StepReferenceMatchResult(0, false);
            }
            else
            {
                // The other text content has less length that this text content,
                // indicating a possible partial match.
                var numberOfMatches = 0;
                while (numberOfMatches < textForRefCompare.Length && textForRefCompare[numberOfMatches] == textForDefCompare[numberOfMatches])
                {
                    numberOfMatches++;
                }

                // Number of character matches in a row equals confidence.
                return new StepReferenceMatchResult(numberOfMatches, false);
            }
        }

        public override bool IsDefinitionPartMatch(DefinitionContentPart part)
        {
            return part is DefinitionContentPart wrd && wrd.Text == Text;
        }

    }

}
