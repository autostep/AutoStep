using System;
using System.Collections.Generic;
using System.Text;
using AutoStep.Elements.Parts;
using AutoStep.Elements.StepTokens;
using FluentAssertions;
using Xunit;

namespace AutoStep.Tests.Compiler.Matching
{
    public class WordDefinitionPartTests
    {
        [Fact]
        public void MatchesSingleRemainingWord()
        {
            var part = new WordDefinitionPart("word");

            var text = "word";

            var tokens = Tokens(new TextToken(0, 4));

            var result = part.DoStepReferenceMatch(text, tokens);

            result.IsExact.Should().BeTrue();
            result.Length.Should().Be(4);
            result.RemainingTokens.Length.Should().Be(0);
        }

        [Fact]
        public void MatchesSingleWordRemainingParts()
        {
            var part = new WordDefinitionPart("word");

            var text = "word other";

            var tokens = Tokens(new TextToken(0, 4), new TextToken(5, 5));

            var result = part.DoStepReferenceMatch(text, tokens);

            result.IsExact.Should().BeTrue();
            result.Length.Should().Be(4);
            result.RemainingTokens.Length.Should().Be(1);
        }

        [Fact]
        public void MatchesMultipleParts()
        {
            var part = new WordDefinitionPart("word123");

            var text = "word123 other";

            var tokens = Tokens(new TextToken(0, 4), new IntToken(4, 3), new TextToken(8, 5));

            var result = part.DoStepReferenceMatch(text, tokens);

            result.IsExact.Should().BeTrue();
            result.Length.Should().Be(7);
            result.RemainingTokens.Length.Should().Be(1);
        }

        [Fact]
        public void FailsMatchingSinglePart()
        {
            var part = new WordDefinitionPart("word");

            var text = "nope";

            var tokens = Tokens(new TextToken(0, 4));

            var result = part.DoStepReferenceMatch(text, tokens);

            result.IsExact.Should().BeFalse();
            result.Length.Should().Be(0);
            result.RemainingTokens.Length.Should().Be(0);
        }

        [Fact]
        public void FailsMatchingSecondPartOfTwoTokens()
        {
            var part = new WordDefinitionPart("word123");

            var text = "word345";

            var tokens = Tokens(new TextToken(0, 4), new IntToken(4, 3));

            var result = part.DoStepReferenceMatch(text, tokens);

            result.IsExact.Should().BeFalse();
            result.Length.Should().Be(4);
            result.RemainingTokens.Length.Should().Be(0);
        }

        [Fact]
        public void PartialMatchOnSubsequentTokens()
        {
            var part = new WordDefinitionPart("word123'really");

            var text = "word123'realnot";

            var tokens = Tokens(
                new TextToken(0, 4), 
                new IntToken(4, 3),
                new QuoteToken(false, 7),
                new TextToken(8, 7)
            );

            var result = part.DoStepReferenceMatch(text, tokens);

            result.IsExact.Should().BeFalse();
            result.Length.Should().Be(12);
            result.RemainingTokens.Length.Should().Be(0);
        }


        [Fact]
        public void MatchesAllTokenTypes()
        {
            var part = new WordDefinitionPart("word123'456.2\":inter<var>");

            var text = "word123'456.2\":inter<var>";

            var tokens = Tokens(
                new TextToken(0, 4),
                new IntToken(4, 3),
                new QuoteToken(false, 7),
                new FloatToken(8, 5),
                new QuoteToken(true, 13),
                new InterpolateStartToken(14),
                new TextToken(15, 5),
                new VariableToken("var", 20, 5)
            );

            var result = part.DoStepReferenceMatch(text, tokens);

            result.IsExact.Should().BeTrue();
            result.Length.Should().Be(text.Length);
            result.RemainingTokens.Length.Should().Be(0);
        }

        private ReadOnlySpan<StepToken> Tokens(params StepToken[] args)
        {
            return args.AsSpan();
        }
    }
}
