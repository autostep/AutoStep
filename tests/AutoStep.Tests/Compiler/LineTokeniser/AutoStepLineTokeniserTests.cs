using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoStep.Compiler;
using AutoStep.Compiler.Parser;
using FluentAssertions;
using Moq;
using Xunit;

namespace AutoStep.Tests.Compiler.LineTokeniser
{
    public class AutoStepLineTokeniserTests
    {
        [Fact]
        public void TokenisesUnboundStep()
        {
            const string Test = "   Given I have <var> something";

            var mockLinker = new Mock<IAutoStepLinker>();

            var lineTokeniser = new AutoStepLineTokeniser(mockLinker.Object);

            var result = lineTokeniser.Tokenise(Test, LineTokeniserState.Default);

            result.EndState.Should().Be(LineTokeniserState.Given);

            result.Tokens.Should().HaveCount(5);

            result.AssertToken(0, 3, LineTokenCategory.StepTypeKeyword, LineTokenSubCategory.Given);
            result.AssertToken(1, 9, LineTokenCategory.StepText, LineTokenSubCategory.Unbound);
            result.AssertToken(2, 11, LineTokenCategory.StepText, LineTokenSubCategory.Unbound);
            result.AssertToken(3, 16, LineTokenCategory.Variable, LineTokenSubCategory.Unbound);
            result.AssertToken(4, 22, LineTokenCategory.StepText, LineTokenSubCategory.Unbound);
        }

        [Fact]
        public void TokenisesUnboundStepWithComment()
        {
            const string Test = "   Given I have # A comment";

            var mockLinker = new Mock<IAutoStepLinker>();

            var lineTokeniser = new AutoStepLineTokeniser(mockLinker.Object);

            var result = lineTokeniser.Tokenise(Test, LineTokeniserState.Default);

            result.EndState.Should().Be(LineTokeniserState.Given);

            result.Tokens.Should().HaveCount(4);

            result.AssertToken(0, 3, LineTokenCategory.StepTypeKeyword, LineTokenSubCategory.Given);
            result.AssertToken(1, 9, LineTokenCategory.StepText, LineTokenSubCategory.Unbound);
            result.AssertToken(2, 11, LineTokenCategory.StepText, LineTokenSubCategory.Unbound);
            result.AssertToken(3, 16, LineTokenCategory.Comment);
        }

        [Fact]
        public void TokenisesCommentOnItsOwn()
        {
            const string Test = " # A line comment";

            var mockLinker = new Mock<IAutoStepLinker>();

            var lineTokeniser = new AutoStepLineTokeniser(mockLinker.Object);

            var result = lineTokeniser.Tokenise(Test, LineTokeniserState.Default);

            result.EndState.Should().Be(LineTokeniserState.Default);

            result.Tokens.Should().HaveCount(1);

            result.AssertToken(0, 1, LineTokenCategory.Comment);
        }


    }

    static class ResultExtensions
    {
        public static void AssertToken(this LineTokeniseResult result, int idx, int column, LineTokenCategory category, LineTokenSubCategory subCategory = LineTokenSubCategory.None)
        {
            var tokenList = result.Tokens.ToList();

            tokenList[idx].StartPosition.Should().Be(column);
            tokenList[idx].Category.Should().Be(category);
            tokenList[idx].SubCategory.Should().Be(subCategory);
        }
    }
}
