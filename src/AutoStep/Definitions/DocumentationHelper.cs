using System;
using System.Text;

namespace AutoStep.Definitions
{
    /// <summary>
    /// Helper functions for processing documentation strings.
    /// </summary>
    internal static class DocumentationHelper
    {
        /// <summary>
        /// Given a block of text, usually defined as a multi-line string in an attribute,
        /// normalises the indentation and padding and produces a processed block of text where the base indentation
        /// has been effectively reset to 0, but with actual indentation preserved.
        /// </summary>
        /// <param name="documentation">The input, raw, documentation block.</param>
        /// <returns>Normalised documentation text.</returns>
        public static string GetProcessedDocumentationBlock(string documentation)
        {
            var text = documentation.AsSpan();

            var builder = new StringBuilder();

            // Go through the content of the doc block.

            // First of all, find the first non-whitespace character.
            var currentPos = 0;
            var knownLineStart = 0;
            var determinedSpacing = false;
            var queuedBlankLines = 0;
            var hitText = false;

            ReadOnlySpan<char> TerminateLine(ReadOnlySpan<char> text)
            {
                if (determinedSpacing)
                {
                    if (currentPos > text.Length)
                    {
                        return text;
                    }

                    var contentToAppend = text.Slice(0, currentPos);

                    if (contentToAppend.Length == 0 || contentToAppend.IsWhiteSpace())
                    {
                        queuedBlankLines++;
                    }
                    else
                    {
                        if (builder.Length > 0)
                        {
                            builder.AppendLine();
                        }

                        while (queuedBlankLines > 0)
                        {
                            builder.AppendLine();

                            queuedBlankLines--;
                        }

                        // Got the content of the line. Append it up until now.
                        builder.Append(text.Slice(0, currentPos));
                    }

                    text = text.Slice(currentPos);
                    hitText = false;
                }

                return text;
            }

            // Get the whitespace characters.
            while (currentPos < text.Length)
            {
                var currentChar = text[currentPos];

                if (currentChar == '\r' || currentChar == '\n')
                {
                    text = TerminateLine(text);

                    if (text[0] == '\r')
                    {
                        // Move on two characters.
                        text = text.Slice(2);
                    }
                    else
                    {
                        text = text.Slice(1);
                    }

                    currentPos = 0;
                }
                else if (!hitText && (!char.IsWhiteSpace(currentChar) || (determinedSpacing && currentPos == knownLineStart)))
                {
                    hitText = true;

                    if (!determinedSpacing)
                    {
                        knownLineStart = currentPos;
                        determinedSpacing = true;
                    }

                    text = text.Slice(currentPos);
                    currentPos = 0;
                }
                else
                {
                    currentPos++;
                }
            }

            TerminateLine(text);

            return builder.ToString();
        }
    }
}
