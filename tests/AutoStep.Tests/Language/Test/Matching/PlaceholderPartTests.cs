using System;
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
            var argPart = new PlaceholderMatchPart(StepPlaceholders.Component);

            argPart.AddMatchingValue("input");
            argPart.AddMatchingValue("button");

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
        public void ComponentsWithMultipleWords()
        {
            var argPart = new PlaceholderMatchPart(StepPlaceholders.Component);

            argPart.AddMatchingValue("text box");

            var text = "text box next";

            var firstPart = WordFromString(text, "text");
            var secondPart = WordFromString(text, "box");
            var thirdPart = WordFromString(text, "next");

            var match = argPart.DoStepReferenceMatch(text, GetContentParts(
                firstPart,
                secondPart,
                thirdPart
            ));

            match.IsExact.Should().Be(true);
            match.Length.Should().Be(8);
            match.GetText(text).Should().Be("text box");
            match.RemainingTokens[0].Should().Be(thirdPart);
        }

        [Fact]
        public void PartialMatchMultiplePlaceholders()
        {
            var argPart = new PlaceholderMatchPart(StepPlaceholders.Component);

            argPart.AddMatchingValue("input");
            argPart.AddMatchingValue("button");

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
            var argPart = new PlaceholderMatchPart(StepPlaceholders.Component);

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
