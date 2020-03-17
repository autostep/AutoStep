using System;
using AutoStep.Elements.Parts;
using AutoStep.Elements.StepTokens;
using AutoStep.Language.Test;

namespace AutoStep.Tests.Utils
{
    internal static class PartExtensions
    {
        public static string GetText(this StepToken part, string text)
        {
            return text.Substring(part.StartIndex, part.Length);
        }

        public static string GetText(this ArgumentBinding binding, string text)
        {
            return GetText(text, binding.MatchedTokens, binding.StartExclusive, binding.EndExclusive);
        }

        public static string GetText(this StepReferenceMatchResult result, string text)
        {
            return GetText(text, result.MatchedTokens, result.StartExclusive, result.EndExclusive);
        }

        private static string GetText(string text, ReadOnlySpan<StepToken> tokens, bool startExclusive, bool endExclusive)
        {
            var first = tokens[0];
            var last = tokens[tokens.Length - 1];
            int startIndex = first.StartIndex;
            int length = (last.StartIndex - startIndex) + last.Length;

            if (tokens.Length > 1)
            {
                if (startExclusive)
                {
                    startIndex += first.Length;
                    length -= first.Length;
                }

                if (endExclusive)
                {
                    length -= last.Length;
                }
            }

            return text.Substring(startIndex, length);
        }
    }
}
