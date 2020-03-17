using AutoStep.Language;
using AutoStep.Language.Interaction;
using AutoStep.Language.Interaction.Parser;
using AutoStep.Tests.Utils;
using FluentAssertions;
using Xunit;

namespace AutoStep.Tests.Language.Interaction.LineTokeniser
{
    public class InteractionLineTokeniserTests
    {
        [Fact]
        public void TokenisesPartialTraitLine()
        {
            var lineTokeniser = new InteractionLineTokeniser();

            var result = lineTokeniser.Tokenise("  Trait: ", 0);

            result.AssertToken(0, 2, LineTokenCategory.EntryMarker, LineTokenSubCategory.InteractionTrait);
        }

        [Fact]
        public void TokenisesComment()
        {
            var lineTokeniser = new InteractionLineTokeniser();

            var result = lineTokeniser.Tokenise(" # Comment ", 0);

            result.AssertToken(0, 1, LineTokenCategory.Comment);
        }

        [Fact]
        public void TokenisesDocComment()
        {
            var lineTokeniser = new InteractionLineTokeniser();

            var result = lineTokeniser.Tokenise(" ## Comment ", 0);

            result.AssertToken(0, 1, LineTokenCategory.Comment);

            result.EndState.Should().Be(0);
        }

        [Fact]
        public void TokenisesTraitLine()
        {
            var lineTokeniser = new InteractionLineTokeniser();

            var result = lineTokeniser.Tokenise("  Trait: clickable + editable", 0);

            result.EndState.Should().Be(0);
            result.AssertToken(0, 2, LineTokenCategory.EntryMarker, LineTokenSubCategory.InteractionTrait);
            result.AssertToken(1, 9, LineTokenCategory.InteractionName);
            result.AssertToken(2, 19, LineTokenCategory.InteractionSeparator);
            result.AssertToken(3, 21, LineTokenCategory.InteractionName);
        }

        [Fact]
        public void TokenisesComponentLine()
        {
            var lineTokeniser = new InteractionLineTokeniser();

            var result = lineTokeniser.Tokenise("  Component: button", 0);

            result.EndState.Should().Be(0);
            result.AssertToken(0, 2, LineTokenCategory.EntryMarker, LineTokenSubCategory.InteractionComponent);
            result.AssertToken(1, 13, LineTokenCategory.InteractionName);
        }

        [Fact]
        public void TokenisesComponentTraits()
        {
            var lineTokeniser = new InteractionLineTokeniser();

            var result = lineTokeniser.Tokenise("  traits: trait1, trait2, trait3", 0);

            result.EndState.Should().Be(0);
            result.AssertToken(0, 2, LineTokenCategory.InteractionPropertyName, LineTokenSubCategory.InteractionTrait);
            result.AssertToken(1, 10, LineTokenCategory.InteractionName);
            result.AssertToken(2, 16, LineTokenCategory.InteractionSeparator);
            result.AssertToken(3, 18, LineTokenCategory.InteractionName);
            result.AssertToken(4, 24, LineTokenCategory.InteractionSeparator);
            result.AssertToken(5, 26, LineTokenCategory.InteractionName);
        }

        [Fact]
        public void TokenisesInheritsKeyword()
        {
            var lineTokeniser = new InteractionLineTokeniser();

            var result = lineTokeniser.Tokenise("  inherits: button", 0);

            result.EndState.Should().Be(0);
            result.AssertToken(0, 2, LineTokenCategory.InteractionPropertyName, LineTokenSubCategory.InteractionInherits);
            result.AssertToken(1, 12, LineTokenCategory.InteractionName);
        }

        [Fact]
        public void TokenisesNameKeyword()
        {
            var lineTokeniser = new InteractionLineTokeniser();

            var result = lineTokeniser.Tokenise("  name: 'button'", 0);

            result.EndState.Should().Be(0);
            result.Tokens.Should().HaveCount(2);
            result.AssertToken(0, 2, LineTokenCategory.InteractionPropertyName, LineTokenSubCategory.InteractionName);
            result.AssertToken(1, 8, LineTokenCategory.InteractionString);
        }

        [Fact]
        public void TokenisesMethodDefinitionNeedsDefining()
        {
            var lineTokeniser = new InteractionLineTokeniser();

            var result = lineTokeniser.Tokenise("  locateNamed(name): needs-defining", 0);

            result.EndState.Should().Be(0);
            result.Tokens.Should().HaveCount(6);
            result.AssertToken(0, 2, LineTokenCategory.InteractionName);
            result.AssertToken(1, 13, LineTokenCategory.InteractionSeparator, LineTokenSubCategory.InteractionParentheses);
            result.AssertToken(2, 14, LineTokenCategory.InteractionArguments, LineTokenSubCategory.InteractionVariable);
            result.AssertToken(3, 18, LineTokenCategory.InteractionSeparator, LineTokenSubCategory.InteractionParentheses);
            result.AssertToken(4, 19, LineTokenCategory.InteractionSeparator);
            result.AssertToken(5, 21, LineTokenCategory.InteractionNeedsDefining);
        }

        [Fact]
        public void TokenisesMethodDefinition()
        {
            var lineTokeniser = new InteractionLineTokeniser();

            var result = lineTokeniser.Tokenise("  locateNamed(name): call(name) -> call()", 0);

            result.EndState.Should().Be(0);
            result.Tokens.Should().HaveCount(13);
            result.AssertToken(0, 2, LineTokenCategory.InteractionName);
            result.AssertToken(1, 13, LineTokenCategory.InteractionSeparator, LineTokenSubCategory.InteractionParentheses);
            result.AssertToken(2, 14, LineTokenCategory.InteractionArguments, LineTokenSubCategory.InteractionVariable);
            result.AssertToken(3, 18, LineTokenCategory.InteractionSeparator, LineTokenSubCategory.InteractionParentheses);
            result.AssertToken(4, 19, LineTokenCategory.InteractionSeparator);
            result.AssertToken(5, 21, LineTokenCategory.InteractionName);
            result.AssertToken(6, 25, LineTokenCategory.InteractionSeparator, LineTokenSubCategory.InteractionParentheses);
            result.AssertToken(7, 26, LineTokenCategory.InteractionArguments, LineTokenSubCategory.InteractionVariable);
            result.AssertToken(8, 30, LineTokenCategory.InteractionSeparator, LineTokenSubCategory.InteractionParentheses);
            result.AssertToken(9, 32, LineTokenCategory.InteractionSeparator, LineTokenSubCategory.InteractionCallSeparator);
            result.AssertToken(10, 35, LineTokenCategory.InteractionName);
            result.AssertToken(11, 39, LineTokenCategory.InteractionSeparator, LineTokenSubCategory.InteractionParentheses);
            result.AssertToken(12, 40, LineTokenCategory.InteractionSeparator, LineTokenSubCategory.InteractionParentheses);
        }

        [Fact]
        public void TokenisesMethodArgs()
        {
            var lineTokeniser = new InteractionLineTokeniser();

            var result = lineTokeniser.Tokenise("  call('123', 123, 0.5, TAB, var, \"str\", 's<var>')", 0);

            result.EndState.Should().Be(0);
            result.Tokens.Should().HaveCount(25);
            result.AssertToken(0, 2, LineTokenCategory.InteractionName);
            result.AssertToken(1, 6, LineTokenCategory.InteractionSeparator, LineTokenSubCategory.InteractionParentheses);
            result.AssertToken(2, 7, LineTokenCategory.InteractionString);
            result.AssertToken(3, 8, LineTokenCategory.InteractionString);
            result.AssertToken(4, 11, LineTokenCategory.InteractionString);
            result.AssertToken(5, 12, LineTokenCategory.InteractionSeparator);
            result.AssertToken(6, 14, LineTokenCategory.InteractionArguments, LineTokenSubCategory.InteractionLiteral);
            result.AssertToken(7, 17, LineTokenCategory.InteractionSeparator);
            result.AssertToken(8, 19, LineTokenCategory.InteractionArguments, LineTokenSubCategory.InteractionLiteral);
            result.AssertToken(9, 22, LineTokenCategory.InteractionSeparator);
            result.AssertToken(10, 24, LineTokenCategory.InteractionArguments, LineTokenSubCategory.InteractionConstant);
            result.AssertToken(11, 27, LineTokenCategory.InteractionSeparator);
            result.AssertToken(12, 29, LineTokenCategory.InteractionArguments, LineTokenSubCategory.InteractionVariable);
            result.AssertToken(13, 32, LineTokenCategory.InteractionSeparator);
            result.AssertToken(14, 34, LineTokenCategory.InteractionString);
            result.AssertToken(15, 35, LineTokenCategory.InteractionString);
            result.AssertToken(16, 38, LineTokenCategory.InteractionString);
            result.AssertToken(17, 39, LineTokenCategory.InteractionSeparator);
            result.AssertToken(18, 41, LineTokenCategory.InteractionString);
            result.AssertToken(19, 42, LineTokenCategory.InteractionString);
            result.AssertToken(20, 43, LineTokenCategory.Variable, LineTokenSubCategory.InteractionVariable);
            result.AssertToken(21, 44, LineTokenCategory.Variable, LineTokenSubCategory.InteractionVariable);
            result.AssertToken(22, 47, LineTokenCategory.Variable, LineTokenSubCategory.InteractionVariable);
            result.AssertToken(23, 48, LineTokenCategory.InteractionString);
            result.AssertToken(24, 49, LineTokenCategory.InteractionSeparator, LineTokenSubCategory.InteractionParentheses);
        }

        [Fact]
        public void TokenisesOpenMethodDefinition()
        {
            var lineTokeniser = new InteractionLineTokeniser();

            var result = lineTokeniser.Tokenise("  locateNamed(name ", 0);

            result.EndState.Should().Be(AutoStepInteractionsLexer.methodArgs);
            result.Tokens.Should().HaveCount(3);
            result.AssertToken(0, 2, LineTokenCategory.InteractionName);
            result.AssertToken(1, 13, LineTokenCategory.InteractionSeparator, LineTokenSubCategory.InteractionParentheses);
            result.AssertToken(2, 14, LineTokenCategory.InteractionArguments, LineTokenSubCategory.InteractionVariable);
        }

        [Fact]
        public void TokenisesStepDefinition()
        {
            var lineTokeniser = new InteractionLineTokeniser();

            var result = lineTokeniser.Tokenise(" Step: Given I have {arg} for $component$", 0);

            result.EndState.Should().Be(0);
            result.Tokens.Should().HaveCount(7);
            result.AssertToken(0, 1, LineTokenCategory.EntryMarker, LineTokenSubCategory.StepDefine);
            result.AssertToken(1, 7, LineTokenCategory.StepTypeKeyword, LineTokenSubCategory.Given);
            result.AssertToken(2, 13, LineTokenCategory.StepText, LineTokenSubCategory.Declaration);
            result.AssertToken(3, 15, LineTokenCategory.StepText, LineTokenSubCategory.Declaration);
            result.AssertToken(4, 20, LineTokenCategory.BoundArgument, LineTokenSubCategory.Declaration);
            result.AssertToken(5, 26, LineTokenCategory.StepText, LineTokenSubCategory.Declaration);
            result.AssertToken(6, 30, LineTokenCategory.Placeholder, LineTokenSubCategory.InteractionComponentPlaceholder);
        }

        [Fact]
        public void TokenisesOpenStringEndsInResetState()
        {
            var lineTokeniser = new InteractionLineTokeniser();

            var result = lineTokeniser.Tokenise("  c(): call('s ", 0);

            result.EndState.Should().Be(0);
            result.Tokens.Should().HaveCount(8);
            result.AssertToken(0, 2, LineTokenCategory.InteractionName);
            result.AssertToken(1, 3, LineTokenCategory.InteractionSeparator, LineTokenSubCategory.InteractionParentheses);
            result.AssertToken(2, 4, LineTokenCategory.InteractionSeparator, LineTokenSubCategory.InteractionParentheses);
            result.AssertToken(3, 5, LineTokenCategory.InteractionSeparator);
            result.AssertToken(4, 7, LineTokenCategory.InteractionName);
            result.AssertToken(5, 11, LineTokenCategory.InteractionSeparator, LineTokenSubCategory.InteractionParentheses);
            result.AssertToken(6, 12, LineTokenCategory.InteractionString);
            result.AssertToken(7, 13, LineTokenCategory.InteractionString);
        }
    }
}
