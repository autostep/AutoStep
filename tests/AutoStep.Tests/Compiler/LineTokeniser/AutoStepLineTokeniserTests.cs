using System;
using System.Linq;
using AutoStep.Compiler;
using AutoStep.Definitions;
using AutoStep.Tests.Utils;
using FluentAssertions;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace AutoStep.Tests.Compiler.LineTokeniser
{
    public class AutoStepLineTokeniserTests : LoggingTestBase
    {
        public AutoStepLineTokeniserTests(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }

        [Fact]
        public void TokenisesFeatureTitle()
        {
            const string Test = "Feature: My Feature";

            var mockLinker = new Mock<IAutoStepLinker>();

            var lineTokeniser = new AutoStepLineTokeniser(mockLinker.Object, true);

            var result = CaptureDiagnostics(() => lineTokeniser.Tokenise(Test, LineTokeniserState.Default));

            result.EndState.Should().Be(LineTokeniserState.EntryBlock);

            result.Tokens.Should().HaveCount(2);

            result.AssertToken(0, 0, LineTokenCategory.EntryMarker, LineTokenSubCategory.Feature);
            result.AssertToken(1, 9, LineTokenCategory.EntityName, LineTokenSubCategory.Feature);
        }

        [Fact]
        public void LiteralTextNoTokens()
        {
            const string Test = "just some text";

            var mockLinker = new Mock<IAutoStepLinker>();

            var lineTokeniser = new AutoStepLineTokeniser(mockLinker.Object, true);

            var result = CaptureDiagnostics(() => lineTokeniser.Tokenise(Test, LineTokeniserState.Default));

            result.EndState.Should().Be(LineTokeniserState.Default);

            result.Tokens.Should().HaveCount(0);
        }

        [Fact]
        public void WhitespaceNoTokens()
        {
            const string Test = "  ";

            var mockLinker = new Mock<IAutoStepLinker>();

            var lineTokeniser = new AutoStepLineTokeniser(mockLinker.Object, true);

            var result = CaptureDiagnostics(() => lineTokeniser.Tokenise(Test, LineTokeniserState.Default));

            result.EndState.Should().Be(LineTokeniserState.Default);

            result.Tokens.Should().HaveCount(0);
        }

        [Fact]
        public void LiteralTextAfterEntryBlockGivesTextToken()
        {
            const string Test = "just some text";

            var mockLinker = new Mock<IAutoStepLinker>();

            var lineTokeniser = new AutoStepLineTokeniser(mockLinker.Object, true);

            var result = CaptureDiagnostics(() => lineTokeniser.Tokenise(Test, LineTokeniserState.EntryBlock));

            result.EndState.Should().Be(LineTokeniserState.EntryBlock);

            result.Tokens.Should().HaveCount(1);

            result.AssertToken(0, 0, LineTokenCategory.Text, LineTokenSubCategory.Description);
        }

        [Fact]
        public void TokenisesScenarioTitle()
        {
            const string Test = "Scenario: My Scenario";

            var mockLinker = new Mock<IAutoStepLinker>();

            var lineTokeniser = new AutoStepLineTokeniser(mockLinker.Object, true);

            var result = CaptureDiagnostics(() => lineTokeniser.Tokenise(Test, LineTokeniserState.Default));

            result.EndState.Should().Be(LineTokeniserState.Default);

            result.Tokens.Should().HaveCount(2);

            result.AssertToken(0, 0, LineTokenCategory.EntryMarker, LineTokenSubCategory.Scenario);
            result.AssertToken(1, 10, LineTokenCategory.EntityName, LineTokenSubCategory.Scenario);
        }

        [Fact]
        public void TokenisesScenarioOutlineTitle()
        {
            const string Test = "Scenario Outline: My Scenario Outline";

            var mockLinker = new Mock<IAutoStepLinker>();

            var lineTokeniser = new AutoStepLineTokeniser(mockLinker.Object, true);

            var result = CaptureDiagnostics(() => lineTokeniser.Tokenise(Test, LineTokeniserState.Default));

            result.EndState.Should().Be(LineTokeniserState.Default);

            result.Tokens.Should().HaveCount(2);

            result.AssertToken(0, 0, LineTokenCategory.EntryMarker, LineTokenSubCategory.ScenarioOutline);
            result.AssertToken(1, 18, LineTokenCategory.EntityName, LineTokenSubCategory.ScenarioOutline);
        }


        [Fact]
        public void TokenisesExamplesEntryPoint()
        {
            const string Test = "Examples:";

            var mockLinker = new Mock<IAutoStepLinker>();

            var lineTokeniser = new AutoStepLineTokeniser(mockLinker.Object, true);

            var result = CaptureDiagnostics(() => lineTokeniser.Tokenise(Test, LineTokeniserState.Default));

            result.EndState.Should().Be(LineTokeniserState.EntryBlock);

            result.Tokens.Should().HaveCount(1);

            result.AssertToken(0, 0, LineTokenCategory.EntryMarker, LineTokenSubCategory.Examples);
        }

        [Fact]
        public void TokenisesBackgroundEntryPoint()
        {
            const string Test = "Background:";

            var mockLinker = new Mock<IAutoStepLinker>();

            var lineTokeniser = new AutoStepLineTokeniser(mockLinker.Object, true);

            var result = CaptureDiagnostics(() => lineTokeniser.Tokenise(Test, LineTokeniserState.Default));

            result.EndState.Should().Be(LineTokeniserState.Default);

            result.Tokens.Should().HaveCount(1);

            result.AssertToken(0, 0, LineTokenCategory.EntryMarker, LineTokenSubCategory.Background);
        }

        [Fact]
        public void TokenisesUnboundStep()
        {
            const string Test = "   Given I have <var> something";

            var mockLinker = new Mock<IAutoStepLinker>();

            var lineTokeniser = new AutoStepLineTokeniser(mockLinker.Object, true);

            var result = CaptureDiagnostics(() => lineTokeniser.Tokenise(Test, LineTokeniserState.Default));

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

            var lineTokeniser = new AutoStepLineTokeniser(mockLinker.Object, true);

            var result = CaptureDiagnostics(() => lineTokeniser.Tokenise(Test, LineTokeniserState.Default));

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

            var lineTokeniser = new AutoStepLineTokeniser(mockLinker.Object, true);

            var result = CaptureDiagnostics(() => lineTokeniser.Tokenise(Test, LineTokeniserState.Default));

            result.EndState.Should().Be(LineTokeniserState.Default);

            result.Tokens.Should().HaveCount(1);

            result.AssertToken(0, 1, LineTokenCategory.Comment);
        }

        [Fact]
        public void TokenisesThenWithArg()
        {
            const string Test = "    Then the 'Client Management - Client Location' page should be displayed";

            var mockLinker = new Mock<IAutoStepLinker>();

            var lineTokeniser = new AutoStepLineTokeniser(mockLinker.Object, true);

            var result = CaptureDiagnostics(() => lineTokeniser.Tokenise(Test, LineTokeniserState.Default));

            result.EndState.Should().Be(LineTokeniserState.Then);
        }

        [Fact]
        public void TokenisesGivenWithArgAndComment()
        {
            const string Test = "    Given I have logged in to my app as 'USER', password 'PWD' # scenario has no description, this is a comment";

            var mockLinker = new Mock<IAutoStepLinker>();

            var lineTokeniser = new AutoStepLineTokeniser(mockLinker.Object, true);

            var result = CaptureDiagnostics(() => lineTokeniser.Tokenise(Test, LineTokeniserState.Default));

            result.Tokens.Should().HaveCount(18);
            result.EndState.Should().Be(LineTokeniserState.Given);
        }

        [Fact]
        public void TokenisesWhen()
        {
            const string Test = "    When I have ";

            var mockLinker = new Mock<IAutoStepLinker>();

            var lineTokeniser = new AutoStepLineTokeniser(mockLinker.Object, true);

            var result = CaptureDiagnostics(() => lineTokeniser.Tokenise(Test, LineTokeniserState.Default));

            result.Tokens.Should().HaveCount(3);
            result.EndState.Should().Be(LineTokeniserState.When);

            result.AssertToken(0, 4, LineTokenCategory.StepTypeKeyword, LineTokenSubCategory.When);
            result.AssertToken(1, 9, LineTokenCategory.StepText, LineTokenSubCategory.Unbound);
            result.AssertToken(2, 11, LineTokenCategory.StepText, LineTokenSubCategory.Unbound);
        }

        [Fact]
        public void TokenisesTag()
        {
            const string Test = "@Tag2";

            var mockLinker = new Mock<IAutoStepLinker>();

            var lineTokeniser = new AutoStepLineTokeniser(mockLinker.Object, true);

            var result = CaptureDiagnostics(() => lineTokeniser.Tokenise(Test, LineTokeniserState.Default));

            result.Tokens.Should().HaveCount(1);
            result.EndState.Should().Be(LineTokeniserState.Default);
            result.AssertToken(0, 0, LineTokenCategory.Annotation, LineTokenSubCategory.Tag);
        }

        [Fact]
        public void TokenisesOption()
        {
            const string Test = "$Option2: Setting 1";

            var mockLinker = new Mock<IAutoStepLinker>();

            var lineTokeniser = new AutoStepLineTokeniser(mockLinker.Object, true);

            var result = CaptureDiagnostics(() => lineTokeniser.Tokenise(Test, LineTokeniserState.Default));

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

            var lineTokeniser = new AutoStepLineTokeniser(linker, true);

            var result = CaptureDiagnostics(() => lineTokeniser.Tokenise(Test, LineTokeniserState.Default));

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

            var lineTokeniser = new AutoStepLineTokeniser(linker, true);

            var result = CaptureDiagnostics(() => lineTokeniser.Tokenise(Test, LineTokeniserState.Given));

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

        [Fact]
        public void TokenisesFirstTableRowAsHeader()
        {
            const string Test = "| header1 | header 2 |";

            var mockLinker = new Mock<IAutoStepLinker>();

            var lineTokeniser = new AutoStepLineTokeniser(mockLinker.Object, true);

            var result = CaptureDiagnostics(() => lineTokeniser.Tokenise(Test, LineTokeniserState.Default));

            result.EndState.Should().Be(LineTokeniserState.TableRow);

            result.Tokens.Should().HaveCount(7);

            result.AssertToken(0, 0, LineTokenCategory.TableBorder);
            result.AssertToken(1, 2, LineTokenCategory.Text, LineTokenSubCategory.Header);
            result.AssertToken(2, 8, LineTokenCategory.Text, LineTokenSubCategory.Header);
            result.AssertToken(3, 10, LineTokenCategory.TableBorder);
            result.AssertToken(4, 12, LineTokenCategory.Text, LineTokenSubCategory.Header);
            result.AssertToken(5, 19, LineTokenCategory.Text, LineTokenSubCategory.Header);
            result.AssertToken(6, 21, LineTokenCategory.TableBorder);
        }

        [Fact]
        public void TokenisesNextTableRowAsNormalRow()
        {
            const string Test = "| header1 | header 2 |";

            var mockLinker = new Mock<IAutoStepLinker>();

            var lineTokeniser = new AutoStepLineTokeniser(mockLinker.Object, true);

            var result = CaptureDiagnostics(() => lineTokeniser.Tokenise(Test, LineTokeniserState.TableRow));

            result.EndState.Should().Be(LineTokeniserState.TableRow);

            result.Tokens.Should().HaveCount(7);

            result.AssertToken(0, 0, LineTokenCategory.TableBorder);
            result.AssertToken(1, 2, LineTokenCategory.Text, LineTokenSubCategory.Cell);
            result.AssertToken(2, 8, LineTokenCategory.Text, LineTokenSubCategory.Cell);
            result.AssertToken(3, 10, LineTokenCategory.TableBorder);
            result.AssertToken(4, 12, LineTokenCategory.Text, LineTokenSubCategory.Cell);
            result.AssertToken(5, 19, LineTokenCategory.Text, LineTokenSubCategory.Cell);
            result.AssertToken(6, 21, LineTokenCategory.TableBorder);
        }


        [Fact]
        public void TokenisesIncompleteTable()
        {
            const string Test = "| header1 | header 2 ";

            var mockLinker = new Mock<IAutoStepLinker>();

            var lineTokeniser = new AutoStepLineTokeniser(mockLinker.Object);

            var result = CaptureDiagnostics(() => lineTokeniser.Tokenise(Test, LineTokeniserState.TableRow));

            result.EndState.Should().Be(LineTokeniserState.TableRow);

            result.Tokens.Should().HaveCount(6);

            result.AssertToken(0, 0, LineTokenCategory.TableBorder);
            result.AssertToken(1, 2, LineTokenCategory.Text, LineTokenSubCategory.Cell);
            result.AssertToken(2, 8, LineTokenCategory.Text, LineTokenSubCategory.Cell);
            result.AssertToken(3, 10, LineTokenCategory.TableBorder);
            result.AssertToken(4, 12, LineTokenCategory.Text, LineTokenSubCategory.Cell);
            result.AssertToken(5, 19, LineTokenCategory.Text, LineTokenSubCategory.Cell);
        }

        [Fact]
        public void TokenisesStepDefinitionHeader()
        {
            const string Test = "Step:";

            var mockLinker = new Mock<IAutoStepLinker>();

            var lineTokeniser = new AutoStepLineTokeniser(mockLinker.Object, true);

            var result = CaptureDiagnostics(() => lineTokeniser.Tokenise(Test, LineTokeniserState.Default));

            result.EndState.Should().Be(LineTokeniserState.EntryBlock);

            result.Tokens.Should().HaveCount(1);

            result.AssertToken(0, 0, LineTokenCategory.EntryMarker, LineTokenSubCategory.StepDefine);            
        }


        [Fact]
        public void TokenisesStepDefinitionTypeOnly()
        {
            const string Test = "Step: Given";

            var mockLinker = new Mock<IAutoStepLinker>();

            var lineTokeniser = new AutoStepLineTokeniser(mockLinker.Object);

            var result = lineTokeniser.Tokenise(Test, LineTokeniserState.Default);

            result.EndState.Should().Be(LineTokeniserState.EntryBlock);

            result.Tokens.Should().HaveCount(2);

            result.AssertToken(0, 0, LineTokenCategory.EntryMarker, LineTokenSubCategory.StepDefine);
            result.AssertToken(1, 6, LineTokenCategory.StepTypeKeyword, LineTokenSubCategory.Given);
        }

        [Fact]
        public void TokenisesStepDefinitionWithArgs()
        {
            const string Test = "Step: Given I {arg} with {arg2}";

            var mockLinker = new Mock<IAutoStepLinker>();

            var lineTokeniser = new AutoStepLineTokeniser(mockLinker.Object);

            var result = lineTokeniser.Tokenise(Test, LineTokeniserState.Default);

            result.EndState.Should().Be(LineTokeniserState.EntryBlock);

            result.Tokens.Should().HaveCount(6);

            result.AssertToken(0, 0, LineTokenCategory.EntryMarker, LineTokenSubCategory.StepDefine);
            result.AssertToken(1, 6, LineTokenCategory.StepTypeKeyword, LineTokenSubCategory.Given);
            result.AssertToken(2, 12, LineTokenCategory.StepText, LineTokenSubCategory.Declaration);
            result.AssertToken(3, 14, LineTokenCategory.BoundArgument, LineTokenSubCategory.Declaration);
            result.AssertToken(4, 20, LineTokenCategory.StepText, LineTokenSubCategory.Declaration);
            result.AssertToken(5, 25, LineTokenCategory.BoundArgument, LineTokenSubCategory.Declaration);
        }

        [Fact]
        public void TokenisesStepDefinitionWhenWithArgs()
        {
            const string Test = "Step: When I {arg} with {arg2}";

            var mockLinker = new Mock<IAutoStepLinker>();

            var lineTokeniser = new AutoStepLineTokeniser(mockLinker.Object);

            var result = lineTokeniser.Tokenise(Test, LineTokeniserState.Default);

            result.EndState.Should().Be(LineTokeniserState.EntryBlock);

            result.Tokens.Should().HaveCount(6);

            result.AssertToken(0, 0, LineTokenCategory.EntryMarker, LineTokenSubCategory.StepDefine);
            result.AssertToken(1, 6, LineTokenCategory.StepTypeKeyword, LineTokenSubCategory.When);
            result.AssertToken(2, 11, LineTokenCategory.StepText, LineTokenSubCategory.Declaration);
            result.AssertToken(3, 13, LineTokenCategory.BoundArgument, LineTokenSubCategory.Declaration);
            result.AssertToken(4, 19, LineTokenCategory.StepText, LineTokenSubCategory.Declaration);
            result.AssertToken(5, 24, LineTokenCategory.BoundArgument, LineTokenSubCategory.Declaration);
        }

        [Fact]
        public void TokenisesStepDefinitionThenWithArgs()
        {
            const string Test = "Step: Then I {arg} with {arg2}";

            var mockLinker = new Mock<IAutoStepLinker>();

            var lineTokeniser = new AutoStepLineTokeniser(mockLinker.Object);

            var result = lineTokeniser.Tokenise(Test, LineTokeniserState.Default);

            result.EndState.Should().Be(LineTokeniserState.EntryBlock);

            result.Tokens.Should().HaveCount(6);

            result.AssertToken(0, 0, LineTokenCategory.EntryMarker, LineTokenSubCategory.StepDefine);
            result.AssertToken(1, 6, LineTokenCategory.StepTypeKeyword, LineTokenSubCategory.Then);
            result.AssertToken(2, 11, LineTokenCategory.StepText, LineTokenSubCategory.Declaration);
            result.AssertToken(3, 13, LineTokenCategory.BoundArgument, LineTokenSubCategory.Declaration);
            result.AssertToken(4, 19, LineTokenCategory.StepText, LineTokenSubCategory.Declaration);
            result.AssertToken(5, 24, LineTokenCategory.BoundArgument, LineTokenSubCategory.Declaration);
        }


        private LineTokeniseResult CaptureDiagnostics(Func<LineTokeniseResult> callback)
        {
            try
            {
                return callback();
            }
            catch (CompilerDiagnosticException ex)
            {
                TestOutput.WriteLine("Diagnostic Error");

                foreach (var item in ex.Errors)
                {
                    TestOutput.WriteLine(item.ToString());
                }

                TestOutput.WriteLine(ex.TokenStreamDetails);

                throw;
            }
        }

    }

    static class TestExtensions
    {
        public static void AssertToken(this LineTokeniseResult result, int idx, int column, LineTokenCategory category, LineTokenSubCategory subCategory = LineTokenSubCategory.None)
        {
            var tokenList = result.Tokens.ToList();

            tokenList[idx].Category.Should().Be(category, "token {0} should have the required category", idx);
            tokenList[idx].SubCategory.Should().Be(subCategory, "token {0} should have the required sub-category", idx);
            tokenList[idx].StartPosition.Should().Be(column, "token {0} should have the required column", idx);
        }
    }
}
