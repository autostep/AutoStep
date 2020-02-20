using System;
using System.Collections.Generic;
using System.Text;
using AutoStep.Elements.Parts;
using AutoStep.Elements.StepTokens;
using AutoStep.Language;
using AutoStep.Tests.Utils;
using FluentAssertions;
using Xunit;

namespace AutoStep.Tests.Language.Test.Matching
{
    public class PlaceholderPartTests
    {
        [Fact]
        public void MatchesOneOfMultiplePlaceholders()
        {
            var argPart = new PlaceholderMatchPart(InteractionPlaceholders.Component);

            argPart.MatchValue("input");
            argPart.MatchValue("button");

            var text = "button next";

            var firstPart = WordFromString(text, "button");
            var secondPart = WordFromString(text, "next");

            var match = argPart.DoStepReferenceMatch(text, GetContentParts(
                firstPart,
                secondPart
            ));

            match.IsExact.Should().Be(true);
            match.Length.Should().Be(6);
            match.GetText(text).Should().Be("button");
            match.RemainingTokens[0].Should().Be(secondPart);
        }

        [Fact]
        public void PartialMatchMultiplePlaceholders()
        {
            var argPart = new PlaceholderMatchPart(InteractionPlaceholders.Component);

            argPart.MatchValue("input");
            argPart.MatchValue("button");

            var text = "but next";

            var firstPart = WordFromString(text, "but");
            var secondPart = WordFromString(text, "next");

            var match = argPart.DoStepReferenceMatch(text, GetContentParts(
                firstPart,
                secondPart
            ));

            match.IsExact.Should().Be(false);
            match.Length.Should().Be(3);
            match.GetText(text).Should().Be("but");
            match.RemainingTokens[0].Should().Be(secondPart);
        }

        [Fact]
        public void NoMatchIfNoComponentValues()
        {
            var argPart = new PlaceholderMatchPart(InteractionPlaceholders.Component);
            
            var text = "button next";

            var firstPart = WordFromString(text, "button");
            var secondPart = WordFromString(text, "next");

            var match = argPart.DoStepReferenceMatch(text, GetContentParts(
                firstPart,
                secondPart
            ));

            match.IsExact.Should().Be(false);
            match.Length.Should().Be(0);
            match.RemainingTokens[0].Should().Be(firstPart);
        }

        private TextToken WordFromString(string text, string subtext)
        {
            var position = text.IndexOf(subtext);

            return new TextToken(position, subtext.Length);
        }

        private ReadOnlySpan<StepToken> GetContentParts(params StepToken[] part)
        {
            return part.AsSpan();
        }
    }
}
