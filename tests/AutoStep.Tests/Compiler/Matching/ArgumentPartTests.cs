using System;
using System.Collections.Generic;
using System.Text;
using AutoStep.Elements.Parts;
using AutoStep.Elements.StepTokens;
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
            match.ResultTokens.GetText(text).Should().Be("sample");
            match.RemainingTokens[0].Should().Be(secondPart);
        }

        [Fact]
        public void MatchesQuotedString()
        {
            var argPart = new ArgumentPart();

            var text = "'foo bah' next";

            var parts = GetContentParts(
                new QuoteToken(0),
                WordFromString(text, "foo"),
                WordFromString(text, "bah"),
                new QuoteToken(8),
                WordFromString(text, "next")
            );

            var match = argPart.DoStepReferenceMatch(text, parts);

            // If there's no 'grouping', then result should just be the one value.
            match.IsExact.Should().Be(true);
            match.Length.Should().Be(4);
            match.ResultTokens.GetText(text).Should().Be("foo bah");
            match.RemainingTokens[0].GetText(text).Should().Be("next");
        }

        [Fact]
        public void QuotedStringInMiddleOfWord()
        {
            var argPart = new ArgumentPart();

            var text = "don't";

            var parts = GetContentParts(
                WordFromString(text, "don"),
                new QuoteToken(3),
                WordFromString(text, "t")
            );

            var match = argPart.DoStepReferenceMatch(text, parts);

            // If there's no 'grouping', then result should just be the one value.
            match.IsExact.Should().Be(true);
            match.Length.Should().Be(3);
            match.ResultTokens.GetText(text).Should().Be("don't");
            match.RemainingTokens.IsEmpty.Should().BeTrue();
        }

        [Fact]
        public void DoubleQuotedStringInMiddleOfSingleQuotedBlock()
        {
            var argPart = new ArgumentPart();

            var text = "'foo \" bah\"' next";

            var parts = GetContentParts(
                new QuoteToken(0),
                WordFromString(text, "foo"),
                new QuoteToken(5) { IsDoubleQuote = true },
                WordFromString(text, "bah"),
                new QuoteToken(10) { IsDoubleQuote = true },
                new QuoteToken(8),
                WordFromString(text, "next")
            );

            var match = argPart.DoStepReferenceMatch(text, parts);

            // If there's no 'grouping', then result should just be the one value.
            match.IsExact.Should().Be(true);
            match.Length.Should().Be(6);
            match.ResultTokens.GetText(text).Should().Be("foo \" bah\"");
            match.RemainingTokens[0].GetText(text).Should().Be("next");
        }

        [Fact]
        public void SingleQuotedStringInMiddleOfDoubleQuotedBlock()
        {
            var argPart = new ArgumentPart();

            var text = "\"foo ' bah'\" next";

            var parts = GetContentParts(
                new QuoteToken(0) { IsDoubleQuote = true },
                WordFromString(text, "foo"),
                new QuoteToken(5),
                WordFromString(text, "bah"),
                new QuoteToken(10),
                new QuoteToken(8) { IsDoubleQuote = true },
                WordFromString(text, "next")
            );

            var match = argPart.DoStepReferenceMatch(text, parts);

            // If there's no 'grouping', then result should just be the one value.
            match.IsExact.Should().Be(true);
            match.Length.Should().Be(6);
            match.ResultTokens.GetText(text).Should().Be("foo ' bah'");
            match.RemainingTokens[0].GetText(text).Should().Be("next");
        }

        private WordToken WordFromString(string text, string subtext)
        {
            var position = text.IndexOf(subtext);

            return new WordToken(position, subtext.Length);
        }

        private ReadOnlySpan<StepToken> GetContentParts(params StepToken[] part)
        {
            return part.AsSpan();
        }
    }
}
