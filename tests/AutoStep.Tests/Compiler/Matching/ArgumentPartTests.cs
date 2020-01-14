using System;
using System.Collections.Generic;
using System.Text;
using AutoStep.Elements.Parts;
using AutoStep.Tests.Builders;
using AutoStep.Tests.Utils;
using FluentAssertions;
using Xunit;

namespace AutoStep.Tests.Compiler.Matching
{
    public class ArgumentPartTests
    {
        [Fact]
        public void MatchesWord()
        {
            var argPart = new ArgumentPart();

            var text = "sample next";

            var firstPart = WordFromString(text, "sample");
            var secondPart = WordFromString(text, "next");

            var match = argPart.DoStepReferenceMatch(text, GetContentParts(
                firstPart,
                secondPart
            ));

            // If there's no 'grouping', then result should just be the one value.
            match.IsExact.Should().Be(true);
            match.Length.Should().Be(1);
            match.ResultParts.GetText(text).Should().Be("sample");
            match.NewSpan[0].Should().Be(secondPart);
        }

        [Fact]
        public void MatchesQuotedString()
        {
            var argPart = new ArgumentPart();

            var text = "'foo bah' next";

            var parts = GetContentParts(
                new QuotePart(0),
                WordFromString(text, "foo"),
                WordFromString(text, "bah"),
                new QuotePart(8),
                WordFromString(text, "next")
            );

            var match = argPart.DoStepReferenceMatch(text, parts);

            // If there's no 'grouping', then result should just be the one value.
            match.IsExact.Should().Be(true);
            match.Length.Should().Be(4);
            match.ResultParts.GetText(text).Should().Be("foo bah");
            match.NewSpan[0].GetText(text).Should().Be("next");
        }

        [Fact]
        public void QuotedStringInMiddleOfWord()
        {
            var argPart = new ArgumentPart();

            var text = "don't";

            var parts = GetContentParts(
                WordFromString(text, "don"),
                new QuotePart(3),
                WordFromString(text, "t")
            );

            var match = argPart.DoStepReferenceMatch(text, parts);

            // If there's no 'grouping', then result should just be the one value.
            match.IsExact.Should().Be(true);
            match.Length.Should().Be(3);
            match.ResultParts.GetText(text).Should().Be("don't");
            match.NewSpan.IsEmpty.Should().BeTrue();
        }

        private WordPart WordFromString(string text, string subtext)
        {
            var position = text.IndexOf(subtext);

            return new WordPart(position, subtext.Length);
        }

        private ReadOnlySpan<ContentPart> GetContentParts(params ContentPart[] part)
        {
            return part.AsSpan();
        }
    }
}
