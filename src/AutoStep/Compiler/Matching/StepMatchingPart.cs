namespace AutoStep.Compiler.Matching
{
    /// <summary>
    /// Defines a single matching part, which is a phrase, word or argument that
    /// is used to match part of a step reference with part of a step definition.
    /// </summary>
    public partial class StepMatchingPart
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StepMatchingPart"/> class, for text content.
        /// </summary>
        /// <param name="textContent">The text to match.</param>
        public StepMatchingPart(string textContent)
        {
            TextContent = textContent ?? throw new System.ArgumentNullException(nameof(textContent));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StepMatchingPart"/> class, for an argument part.
        /// </summary>
        /// <param name="argument">The argument type.</param>
        public StepMatchingPart(ArgumentType argument)
        {
            ArgumentType = argument;
        }

        /// <summary>
        /// Gets the text content of the part (if this is a text part).
        /// </summary>
        public string? TextContent { get; }

        /// <summary>
        /// Gets the type of the argument (if this is an argument part).
        /// </summary>
        public ArgumentType? ArgumentType { get; }

        /// <summary>
        /// Gets a value indicating whether this is a text part.
        /// </summary>
        public bool IsText => TextContent is string;

        /// <summary>
        /// Gets a value indicating whether this is an argument part.
        /// </summary>
        public bool IsArgument => ArgumentType.HasValue;

        /// <summary>
        /// Checks whether this part is an exact match for another.
        /// </summary>
        /// <param name="other">The other part.</param>
        /// <returns>True if the match is exact.</returns>
        public bool IsExactMatch(StepMatchingPart other)
        {
            if (other is null)
            {
                throw new System.ArgumentNullException(nameof(other));
            }

            return IsArgument && ArgumentType == other.ArgumentType ||
                   TextContent == other.TextContent;
        }

        /// <summary>
        /// Does an approximate match on the provided other matching part.
        /// </summary>
        /// <param name="other">The type to check against.</param>
        /// <returns>The result of the match.</returns>
        /// <remarks>It's expected that the 'other' will be a reference part, and the current instance is a definition part.</remarks>
        public ApproximateMatchResult ApproximateMatch(StepMatchingPart other)
        {
            if (other is null)
            {
                throw new System.ArgumentNullException(nameof(other));
            }

            if (IsArgument)
            {
                if (other.IsArgument)
                {
                    var diff = other.ArgumentType!.Value - ArgumentType!.Value;

                    // Negative values indicate a non-compatible argument type.
                    // Everything can go into Text, which is 0.
                    if (diff < 0)
                    {
                        return new ApproximateMatchResult(0, false);
                    }
                    else
                    {
                        // Argument match.
                        return new ApproximateMatchResult(diff + 1, true);
                    }
                }
                else
                {
                    return new ApproximateMatchResult(0, false);
                }
            }
            else
            {
                if (other.IsText)
                {
                    if (other.TextContent!.Length == TextContent!.Length)
                    {
                        // Exact length match. Do a straight-forward compare.
                        if (other.TextContent == TextContent)
                        {
                            // Exact match.
                            return new ApproximateMatchResult(TextContent.Length, true);
                        }
                        else
                        {
                            return new ApproximateMatchResult(0, false);
                        }
                    }
                    else if (other.TextContent.Length > TextContent.Length)
                    {
                        // Not possible to match, the search target contains more text than this part.
                        return new ApproximateMatchResult(0, false);
                    }
                    else
                    {
                        // The other text content has less length that this text content,
                        // indicating a possible partial match.
                        var numberOfMatches = 0;
                        while (numberOfMatches < other.TextContent.Length && other.TextContent[numberOfMatches] == TextContent[numberOfMatches])
                        {
                            numberOfMatches++;
                        }

                        // Number of character matches in a row equals confidence.
                        return new ApproximateMatchResult(numberOfMatches, false);
                    }
                }
                else
                {
                    return new ApproximateMatchResult(0, false);
                }
            }
        }
    }
}
