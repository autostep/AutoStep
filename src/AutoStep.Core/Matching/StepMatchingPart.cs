using System.Collections.Generic;

namespace AutoStep.Core.Matching
{
    public class StepMatchingPart
    {
        public StepMatchingPart(string textContent)
        {
            TextContent = textContent ?? throw new System.ArgumentNullException(nameof(textContent));
        }

        public StepMatchingPart(ArgumentType argument)
        {
            ArgumentType = argument;
        }

        public string TextContent { get; }

        public bool IsText => TextContent is string;

        public bool IsArgument => ArgumentType.HasValue;

        public ArgumentType? ArgumentType { get; set; }

        public bool IsExactMatch(StepMatchingPart other)
        {
            return (IsArgument && ArgumentType == other.ArgumentType) ||
                   (TextContent == other.TextContent);
        }

        public (int length, bool isExact) ApproximateMatch(StepMatchingPart other)
        {
            if (other is null)
            {
                throw new System.ArgumentNullException(nameof(other));
            }

            if (IsArgument)
            {
                if (other.IsArgument)
                {
                    var diff = other.ArgumentType.Value - ArgumentType.Value;

                    // Negative values indicate a non-compatible argument type.
                    // Everything can go into Text, which is 0.
                    if (diff < 0)
                    {
                        return (0, false);
                    }
                    else
                    {
                        // Argument match.
                        return (diff + 1, true);
                    }
                }
                else
                {
                    return (0, false);
                }
            }
            else
            {
                if (other.IsText)
                {
                    if (other.TextContent.Length == TextContent.Length)
                    {
                        // Exact length match. Do a straight-forward compare.
                        if (other.TextContent == TextContent)
                        {
                            // Exact match.
                            return (TextContent.Length, true);
                        }
                        else
                        {
                            return (0, false);
                        }
                    }
                    else if (other.TextContent.Length > TextContent.Length)
                    {
                        // Not possible to match, the search target contains more text than this part.
                        return (0, false);
                    }
                    else
                    {
                        // The other text content has less length that this text content,
                        // indicating a possible partial match.
                        int numberOfMatches = 0;
                        while (numberOfMatches < other.TextContent.Length && other.TextContent[numberOfMatches] == TextContent[numberOfMatches])
                        {
                            numberOfMatches++;
                        }

                        // Number of character matches in a row equals confidence.
                        return (numberOfMatches, false);
                    }
                }
                else
                {
                    return (0, false);
                }
            }
        }
    }
}
