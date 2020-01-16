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
            match.GetText(text).Should().Be("sample");
            match.RemainingTokens[0].Should().Be(secondPart);
        }

        [Fact]
        public void MatchesQuotedString()
        {
            var argPart = new ArgumentPart();

            var text = "'foo bah' next";

            var parts = GetContentParts(
                new QuoteToken(false, 0),
                WordFromString(text, "foo"),
                WordFromString(text, "bah"),
                new QuoteToken(false, 8),
                WordFromString(text, "next")
            );

            var match = argPart.DoStepReferenceMatch(text, parts);

            match.IsExact.Should().Be(true);
            match.Length.Should().Be(4);
            match.GetText(text).Should().Be("foo bah");
            match.RemainingTokens[0].GetText(text).Should().Be("next");
        }
        
        [Fact]
        public void UnterminatedQuotedString()
        {
            var argPart = new ArgumentPart();

            var text = "'foo bah next";

            var parts = GetContentParts(
                new QuoteToken(false, 0),
                WordFromString(text, "foo"),
                WordFromString(text, "bah"),
                WordFromString(text, "next")
            );

            var match = argPart.DoStepReferenceMatch(text, parts);

            // Unterminated quoted arguments cause the capture of the entire remaining tokens.
            match.IsExact.Should().Be(true);
            match.Length.Should().Be(4);
            match.GetText(text).Should().Be("foo bah next");
            match.RemainingTokens.IsEmpty.Should().BeTrue();
        }

        [Fact]
        public void QuotedStringInMiddleOfWord()
        {
            var argPart = new ArgumentPart();

            var text = "don't";

            var parts = GetContentParts(
                WordFromString(text, "don"),
                new QuoteToken(false, 3),
                WordFromString(text, "t")
            );

            var match = argPart.DoStepReferenceMatch(text, parts);

            // If there's no 'grouping', then result should just be the one value.
            match.IsExact.Should().Be(true);
            match.Length.Should().Be(3);
            match.GetText(text).Should().Be("don't");
            match.RemainingTokens.IsEmpty.Should().BeTrue();
        }

        [Fact]
        public void DoubleQuotedStringInMiddleOfSingleQuotedBlock()
        {
            var argPart = new ArgumentPart();

            var text = "'foo \" bah\"' next";

            var parts = GetContentParts(
                new QuoteToken(false, 0),
                WordFromString(text, "foo"),
                new QuoteToken(true, 5),
                WordFromString(text, "bah"),
                new QuoteToken(true, 10),
                new QuoteToken(false, 11),
                WordFromString(text, "next")
            );

            var match = argPart.DoStepReferenceMatch(text, parts);

            // If there's no 'grouping', then result should just be the one value.
            match.IsExact.Should().Be(true);
            match.Length.Should().Be(6);
            match.GetText(text).Should().Be("foo \" bah\"");
            match.RemainingTokens[0].GetText(text).Should().Be("next");
        }

        [Fact]
        public void SingleQuotedStringInMiddleOfDoubleQuotedBlock()
        {
            var argPart = new ArgumentPart();

            var text = "\"foo ' bah'\" next";

            var parts = GetContentParts(
                new QuoteToken(true, 0),
                WordFromString(text, "foo"),
                new QuoteToken(false, 5),
                WordFromString(text, "bah"),
                new QuoteToken(false, 10),
                new QuoteToken(true, 11),
                WordFromString(text, "next")
            );

            var match = argPart.DoStepReferenceMatch(text, parts);

            // If there's no 'grouping', then result should just be the one value.
            match.IsExact.Should().Be(true);
            match.Length.Should().Be(6);
            match.GetText(text).Should().Be("foo ' bah'");
            match.RemainingTokens[0].GetText(text).Should().Be("next");
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
