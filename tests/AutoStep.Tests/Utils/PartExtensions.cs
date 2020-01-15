using System;
using System.Collections.Generic;
using System.Text;
using AutoStep.Elements.StepTokens;

namespace AutoStep.Tests.Utils
{
    internal static class PartExtensions
    {
        public static string GetText(this StepToken part, string text)
        {
            return text.Substring(part.StartIndex, part.Length);
        }

        public static string GetText(this ReadOnlySpan<StepToken> parts, string text)
        {
            var firstSpan = parts[0];
            var lastSpan = parts[parts.Length - 1];

            return text.Substring(firstSpan.StartIndex, (lastSpan.StartIndex - firstSpan.StartIndex) + lastSpan.Length);
        }
    }
}
