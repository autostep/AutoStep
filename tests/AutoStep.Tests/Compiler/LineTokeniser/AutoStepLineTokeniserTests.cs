using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoStep.Compiler;
using AutoStep.Compiler.Parser;
using AutoStep.Definitions;
using AutoStep.Elements;
using AutoStep.Elements.Parts;
using AutoStep.Execution;
using AutoStep.Execution.Contexts;
using AutoStep.Execution.Dependency;
using AutoStep.Tests.Builders;
using AutoStep.Tests.Utils;
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

        [Fact]
        public void TokenisesThenWithArg()
        {
            const string Test = "    Then the 'Client Management - Client Location' page should be displayed";

            var mockLinker = new Mock<IAutoStepLinker>();

            var lineTokeniser = new AutoStepLineTokeniser(mockLinker.Object);

            var result = lineTokeniser.Tokenise(Test, LineTokeniserState.Default);

            result.EndState.Should().Be(LineTokeniserState.Then);
        }

        [Fact]
        public void TokenisesGivenWithArgAndComment()
        {
            const string Test = "    Given I have logged in to my app as 'USER', password 'PWD' # scenario has no description, this is a comment";

            var mockLinker = new Mock<IAutoStepLinker>();

            var lineTokeniser = new AutoStepLineTokeniser(mockLinker.Object);

            var result = lineTokeniser.Tokenise(Test, LineTokeniserState.Default);

            result.Tokens.Should().HaveCount(18);
            result.EndState.Should().Be(LineTokeniserState.Given);
        }

        [Fact]
        public void TokenisesTag()
        {
            const string Test = "@Tag2";

            var mockLinker = new Mock<IAutoStepLinker>();

            var lineTokeniser = new AutoStepLineTokeniser(mockLinker.Object);

            var result = lineTokeniser.Tokenise(Test, LineTokeniserState.Default);

            result.Tokens.Should().HaveCount(1);
            result.EndState.Should().Be(LineTokeniserState.Default);
            result.AssertToken(0, 0, LineTokenCategory.Annotation, LineTokenSubCategory.Tag);
        }

        [Fact]
        public void TokenisesOption()
        {
            const string Test = "$Option2: Setting 1";

            var mockLinker = new Mock<IAutoStepLinker>();

            var lineTokeniser = new AutoStepLineTokeniser(mockLinker.Object);

            var result = lineTokeniser.Tokenise(Test, LineTokeniserState.Default);

            result.Tokens.Should().HaveCount(1);
            result.EndState.Should().Be(LineTokeniserState.Default);
            result.AssertToken(0, 0, LineTokenCategory.Annotation, LineTokenSubCategory.Option);
        }

        [Fact]
        public void TokenisesBoundStep()
        {
            const string Test = "Given I have entered 'My Name' into 'Name'";

            var compiler = new AutoStepCompiler();
            var linker = new AutoStepLinker(compiler);

            var source = new CallbackDefinitionSource();
            source.Given("I have entered {arg1} into {arg2}", () => { });

            linker.AddStepDefinitionSource(source);

            var lineTokeniser = new AutoStepLineTokeniser(linker);

            var result = lineTokeniser.Tokenise(Test, LineTokeniserState.Default);

            result.Tokens.Should().HaveCount(12);
            result.EndState.Should().Be(LineTokeniserState.Given);
            result.AssertToken(0, 0, LineTokenCategory.StepTypeKeyword, LineTokenSubCategory.Given);
            result.AssertToken(1, 6, LineTokenCategory.StepText, LineTokenSubCategory.Bound);
            result.AssertToken(2, 8, LineTokenCategory.StepText, LineTokenSubCategory.Bound);
            result.AssertToken(3, 13, LineTokenCategory.StepText, LineTokenSubCategory.Bound);
            result.AssertToken(4, 21, LineTokenCategory.BoundArgument);
            result.AssertToken(5, 22, LineTokenCategory.BoundArgument);
            result.AssertToken(6, 25, LineTokenCategory.BoundArgument);
            result.AssertToken(7, 29, LineTokenCategory.BoundArgument);
            result.AssertToken(8, 31, LineTokenCategory.StepText, LineTokenSubCategory.Bound);
            result.AssertToken(9, 36, LineTokenCategory.BoundArgument);
            result.AssertToken(10, 37, LineTokenCategory.BoundArgument);
            result.AssertToken(11, 41, LineTokenCategory.BoundArgument);
        }

        [Fact]
        public void TokenisesBoundAndFollowingAGiven()
        {
            const string Test = "And I have entered 'My Name'";

            var compiler = new AutoStepCompiler();
            var linker = new AutoStepLinker(compiler);

            var source = new CallbackDefinitionSource();
            source.Given("I have entered {arg1}", () => { });

            linker.AddStepDefinitionSource(source);

            var lineTokeniser = new AutoStepLineTokeniser(linker);

            var result = lineTokeniser.Tokenise(Test, LineTokeniserState.Given);

            result.Tokens.Should().HaveCount(8);
            result.EndState.Should().Be(LineTokeniserState.Given);
            result.AssertToken(0, 0, LineTokenCategory.StepTypeKeyword, LineTokenSubCategory.And);
            result.AssertToken(1, 4, LineTokenCategory.StepText, LineTokenSubCategory.Bound);
            result.AssertToken(2, 6, LineTokenCategory.StepText, LineTokenSubCategory.Bound);
            result.AssertToken(3, 11, LineTokenCategory.StepText, LineTokenSubCategory.Bound);
            result.AssertToken(4, 19, LineTokenCategory.BoundArgument);
            result.AssertToken(5, 20, LineTokenCategory.BoundArgument);
            result.AssertToken(6, 23, LineTokenCategory.BoundArgument);
            result.AssertToken(7, 27, LineTokenCategory.BoundArgument);
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
