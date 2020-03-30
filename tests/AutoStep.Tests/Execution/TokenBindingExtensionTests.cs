using AutoStep.Elements.Parts;
using AutoStep.Execution;
using AutoStep.Execution.Dependency;
using AutoStep.Tests.Builders;
using FluentAssertions;
using Moq;
using Xunit;
using AutoStep.Language.Test;

namespace AutoStep.Tests.Execution
{
    public class TokenBindingExtensionTests
    {
        [Fact]
        public void BindWordAndIntArgument()
        {
            var stepDef = new StepDefinitionBuilder(StepType.Given, "I {arg}", 1, 1);
            stepDef.Argument("{arg}", "arg", 3);

            var stepRef = new StepReferenceBuilder("I argument1", StepType.Given, StepType.Given, 1, 1);
            stepRef.Text("I");
            stepRef.Text("argument");
            stepRef.Int("1");
            stepRef.Built.FreezeTokens();

            var argTokens = stepRef.Built.TokenSpan.Slice(1);

            var matchResult = new StepReferenceMatchResult(1, true, default, argTokens);

            var binding = new ArgumentBinding(stepDef.Built.Arguments[0], matchResult);

            var fakeScope = new Mock<IServiceScope>().Object;

            var text = binding.GetFullText(fakeScope, "I argument1", VariableSet.Blank);

            text.Should().Be("argument1");
        }

        [Fact]
        public void BindWordArgument()
        {
            var stepDef = new StepDefinitionBuilder(StepType.Given, "I {arg}", 1, 1);
            stepDef.Argument("{arg}", "arg", 3);

            var stepRef = new StepReferenceBuilder("I argument", StepType.Given, StepType.Given, 1, 1);
            stepRef.Text("I");
            stepRef.Text("argument");
            stepRef.Built.FreezeTokens();

            var argTokens = stepRef.Built.TokenSpan.Slice(1);

            var matchResult = new StepReferenceMatchResult(1, true, default, argTokens);

            var binding = new ArgumentBinding(stepDef.Built.Arguments[0], matchResult);

            var fakeScope = new Mock<IServiceScope>().Object;

            var text = binding.GetFullText(fakeScope, "I argument", VariableSet.Blank);

            text.Should().Be("argument");
        }

        [Fact]
        public void BindQuotedSingleWordArgument()
        {
            var stepDef = new StepDefinitionBuilder(StepType.Given, "I {arg}", 1, 1);
            stepDef.Argument("{arg}", "arg", 3);

            var stepRef = new StepReferenceBuilder("I 'argument'", StepType.Given, StepType.Given, 1, 1);
            stepRef.Text("I");
            stepRef.Quote();
            stepRef.Text("argument");
            stepRef.Quote();
            stepRef.Built.FreezeTokens();

            var argTokens = stepRef.Built.TokenSpan.Slice(1);

            var matchResult = new StepReferenceMatchResult(1, true, default, argTokens, true, true);

            var binding = new ArgumentBinding(stepDef.Built.Arguments[0], matchResult);

            var fakeScope = new Mock<IServiceScope>().Object;

            var text = binding.GetFullText(fakeScope, "I 'argument'", VariableSet.Blank);

            text.Should().Be("argument");
        }

        [Fact]
        public void BindQuotedMultiTokenArgument()
        {
            var stepDef = new StepDefinitionBuilder(StepType.Given, "I {arg}", 1, 1);
            stepDef.Argument("{arg}", "arg", 3);

            var stepRef = new StepReferenceBuilder("I 'argument something'", StepType.Given, StepType.Given, 1, 1);
            stepRef.Text("I");
            stepRef.Quote();
            stepRef.Text("argument");
            stepRef.Text("something");
            stepRef.Quote();
            stepRef.Built.FreezeTokens();

            var argTokens = stepRef.Built.TokenSpan.Slice(1);

            var matchResult = new StepReferenceMatchResult(1, true, default, argTokens, true, true);

            var binding = new ArgumentBinding(stepDef.Built.Arguments[0], matchResult);

            var fakeScope = new Mock<IServiceScope>().Object;

            var text = binding.GetFullText(fakeScope, "I 'argument something'", VariableSet.Blank);

            text.Should().Be("argument something");
        }

        [Fact]
        public void BindUnclosedQuotedMultiTokenArgument()
        {
            var stepDef = new StepDefinitionBuilder(StepType.Given, "I {arg}", 1, 1);
            stepDef.Argument("{arg}", "arg", 3);

            var stepRef = new StepReferenceBuilder("I 'argument something'", StepType.Given, StepType.Given, 1, 1);
            stepRef.Text("I");
            stepRef.Quote();
            stepRef.Text("argument");
            stepRef.Text("something");
            stepRef.Built.FreezeTokens();

            var argTokens = stepRef.Built.TokenSpan.Slice(1);

            var matchResult = new StepReferenceMatchResult(1, true, default, argTokens, true, false);

            var binding = new ArgumentBinding(stepDef.Built.Arguments[0], matchResult);

            var fakeScope = new Mock<IServiceScope>().Object;

            var text = binding.GetFullText(fakeScope, "I 'argument something", VariableSet.Blank);

            text.Should().Be("argument something");
        }

        [Fact]
        public void BindVariableOnly()
        {
            var stepDef = new StepDefinitionBuilder(StepType.Given, "I {arg}", 1, 1);
            stepDef.Argument("{arg}", "arg", 3);

            var stepRef = new StepReferenceBuilder("I <var>", StepType.Given, StepType.Given, 1, 1);
            stepRef.Text("I");
            stepRef.Variable("var");
            stepRef.Built.FreezeTokens();

            var argTokens = stepRef.Built.TokenSpan.Slice(1);

            var matchResult = new StepReferenceMatchResult(9, true, default, argTokens);

            var binding = new ArgumentBinding(stepDef.Built.Arguments[0], matchResult);

            var fakeScope = new Mock<IServiceScope>().Object;

            var variableSet = new VariableSet();
            variableSet.Set("var", "value"); ;

            var text = binding.GetFullText(fakeScope, "I <var>", variableSet);

            text.Should().Be("value");
        }

        [Fact]
        public void BindVariableAmongstTokens()
        {
            var stepDef = new StepDefinitionBuilder(StepType.Given, "I {arg}", 1, 1);
            stepDef.Argument("{arg}", "arg", 3);

            var stepRef = new StepReferenceBuilder("I 'word1 <var> word2'", StepType.Given, StepType.Given, 1, 1);
            stepRef.Text("I").Quote().Text("word").Int("1").Variable("var").Text("word").Int("2").Quote();
            stepRef.Built.FreezeTokens();

            var argTokens = stepRef.Built.TokenSpan.Slice(1);

            var matchResult = new StepReferenceMatchResult(9, true, default, argTokens, true, true);

            var binding = new ArgumentBinding(stepDef.Built.Arguments[0], matchResult);

            var fakeScope = new Mock<IServiceScope>().Object;

            var variableSet = new VariableSet();
            variableSet.Set("var", "value"); ;

            var text = binding.GetFullText(fakeScope, stepRef.Built.RawText, variableSet);

            text.Should().Be("word1 value word2");
        }

        [Fact]
        public void BindTokenWhitespaceLeft()
        {
            var stepDef = new StepDefinitionBuilder(StepType.Given, "I {arg}", 1, 1);
            stepDef.Argument("{arg}", "arg", 3);

            var stepRef = new StepReferenceBuilder("I ' word1 <var> word2'", StepType.Given, StepType.Given, 1, 1);
            stepRef.Text("I").Quote().Text("word").Int("1").Variable("var").Text("word").Int("2").Quote();
            stepRef.Built.FreezeTokens();

            var argTokens = stepRef.Built.TokenSpan.Slice(1);

            var matchResult = new StepReferenceMatchResult(9, true, default, argTokens, true, true);

            var binding = new ArgumentBinding(stepDef.Built.Arguments[0], matchResult);

            var fakeScope = new Mock<IServiceScope>().Object;

            var variableSet = new VariableSet();
            variableSet.Set("var", "value"); ;

            var text = binding.GetFullText(fakeScope, stepRef.Built.RawText, variableSet);

            text.Should().Be(" word1 value word2");
        }

        [Fact]
        public void BindTokenWhitespaceRight()
        {
            var stepDef = new StepDefinitionBuilder(StepType.Given, "I {arg}", 1, 1);
            stepDef.Argument("{arg}", "arg", 3);

            var stepRef = new StepReferenceBuilder("I 'word1 <var> word2 '", StepType.Given, StepType.Given, 1, 1);
            stepRef.Text("I").Quote().Text("word").Int("1").Variable("var").Text("word").Int("2").Quote();
            stepRef.Built.FreezeTokens();

            var argTokens = stepRef.Built.TokenSpan.Slice(1);

            var matchResult = new StepReferenceMatchResult(9, true, default, argTokens, true, true);

            var binding = new ArgumentBinding(stepDef.Built.Arguments[0], matchResult);

            var fakeScope = new Mock<IServiceScope>().Object;

            var variableSet = new VariableSet();
            variableSet.Set("var", "value"); ;

            var text = binding.GetFullText(fakeScope, stepRef.Built.RawText, variableSet);

            text.Should().Be("word1 value word2 ");
        }

        [Fact]
        public void BindTokenWhitespaceBoth()
        {
            var stepDef = new StepDefinitionBuilder(StepType.Given, "I {arg}", 1, 1);
            stepDef.Argument("{arg}", "arg", 3);

            var stepRef = new StepReferenceBuilder("I ' word1 <var> word2 '", StepType.Given, StepType.Given, 1, 1);
            stepRef.Text("I").Quote().Text("word").Int("1").Variable("var").Text("word").Int("2").Quote();
            stepRef.Built.FreezeTokens();

            var argTokens = stepRef.Built.TokenSpan.Slice(1);

            var matchResult = new StepReferenceMatchResult(9, true, default, argTokens, true, true);

            var binding = new ArgumentBinding(stepDef.Built.Arguments[0], matchResult);

            var fakeScope = new Mock<IServiceScope>().Object;

            var variableSet = new VariableSet();
            variableSet.Set("var", "value"); ;

            var text = binding.GetFullText(fakeScope, stepRef.Built.RawText, variableSet);

            text.Should().Be(" word1 value word2 ");
        }

        [Fact(Skip = "Interpolation not yet implemented")]
        public void BindInterpolation()
        {
            var stepDef = new StepDefinitionBuilder(StepType.Given, "I {arg}", 1, 1);
            stepDef.Argument("{arg}", "arg", 3);

            var stepRef = new StepReferenceBuilder("I :now", StepType.Given, StepType.Given, 1, 1);
            stepRef.Text("I").InterpolateStart().Text("now");
            stepRef.Built.FreezeTokens();

            var argTokens = stepRef.Built.TokenSpan.Slice(1);

            var matchResult = new StepReferenceMatchResult(9, true, default, argTokens, true, true);

            var binding = new ArgumentBinding(stepDef.Built.Arguments[0], matchResult);

            var fakeScope = new Mock<IServiceScope>().Object;

            var text = binding.GetFullText(fakeScope, stepRef.Built.RawText, VariableSet.Blank);

            text.Should().Be("?");
        }

        [Fact]
        public void CellBindWordAndIntValue()
        {
            var stepDef = new CellBuilder("value1", 1, 1, 7);
            stepDef.Text("value").Int("1");

            var fakeScope = new Mock<IServiceScope>().Object;

            var text = stepDef.Built.GetFullText(fakeScope, VariableSet.Blank);

            text.Should().Be("value1");
        }

        [Fact]
        public void CellBindWhiteSpace()
        {
            var stepDef = new CellBuilder("value 1", 1, 1, 7);
            stepDef.Text("value").Int("1");

            var fakeScope = new Mock<IServiceScope>().Object;

            var text = stepDef.Built.GetFullText(fakeScope, VariableSet.Blank);

            text.Should().Be("value 1");
        }

        [Fact]
        public void CellBindVariable()
        {
            var stepDef = new CellBuilder("value <var>", 1, 1, 7);
            stepDef.Text("value").Variable("var");

            var fakeScope = new Mock<IServiceScope>().Object;
            var variables = new VariableSet();
            variables.Set("var", "something");

            var text = stepDef.Built.GetFullText(fakeScope, variables);

            text.Should().Be("value something");
        }

        [Fact]
        public void GetRawTextQuotedMultiTokenArgument()
        {
            var stepDef = new StepDefinitionBuilder(StepType.Given, "I {arg}", 1, 1);
            stepDef.Argument("{arg}", "arg", 3);

            var stepRef = new StepReferenceBuilder("I 'argument something'", StepType.Given, StepType.Given, 1, 1);
            stepRef.Text("I");
            stepRef.Quote();
            stepRef.Text("argument");
            stepRef.Text("something");
            stepRef.Quote();
            stepRef.Built.FreezeTokens();

            var argTokens = stepRef.Built.TokenSpan.Slice(1);

            var matchResult = new StepReferenceMatchResult(1, true, default, argTokens, true, true);

            var binding = new ArgumentBinding(stepDef.Built.Arguments[0], matchResult);

            var text = binding.GetRawText("I 'argument something'");

            text.Should().Be("argument something");
        }

        [Fact]
        public void GetRawTextLengthQuotedMultiTokenArgument()
        {
            var stepDef = new StepDefinitionBuilder(StepType.Given, "I {arg}", 1, 1);
            stepDef.Argument("{arg}", "arg", 3);

            var stepRef = new StepReferenceBuilder("I 'argument something'", StepType.Given, StepType.Given, 1, 1);
            stepRef.Text("I");
            stepRef.Quote();
            stepRef.Text("argument");
            stepRef.Text("something");
            stepRef.Quote();
            stepRef.Built.FreezeTokens();

            var argTokens = stepRef.Built.TokenSpan.Slice(1);

            var matchResult = new StepReferenceMatchResult(1, true, default, argTokens, true, true);

            var binding = new ArgumentBinding(stepDef.Built.Arguments[0], matchResult);

            var length = binding.GetRawLength();

            length.Should().Be(18);
        }

        [Fact]
        public void GetRawTextVariableAmongstTokens()
        {
            var stepDef = new StepDefinitionBuilder(StepType.Given, "I {arg}", 1, 1);
            stepDef.Argument("{arg}", "arg", 3);

            var stepRef = new StepReferenceBuilder("I 'word1 <var> word2'", StepType.Given, StepType.Given, 1, 1);
            stepRef.Text("I").Quote().Text("word").Int("1").Variable("var").Text("word").Int("2").Quote();
            stepRef.Built.FreezeTokens();

            var argTokens = stepRef.Built.TokenSpan.Slice(1);

            var matchResult = new StepReferenceMatchResult(9, true, default, argTokens, true, true);

            var binding = new ArgumentBinding(stepDef.Built.Arguments[0], matchResult);

            var text = binding.GetRawText("I 'word1 <var> word2'");

            text.Should().Be("word1 <var> word2");
        }
    }
}
