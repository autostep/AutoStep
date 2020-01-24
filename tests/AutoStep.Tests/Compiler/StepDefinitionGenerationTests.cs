using AutoStep.Compiler;
using AutoStep.Tests.Builders;
using AutoStep.Tests.Utils;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace AutoStep.Tests.Compiler
{
    public class StepDefinitionGenerationTests : CompilerTestBase
    {
        public StepDefinitionGenerationTests(ITestOutputHelper output)
            : base(output)
        {
        }

        [Fact]
        public void GenerateStepDefinitionNoArguments()
        {
            const string TestStep = "I have done something";

            var compiler = new AutoStepCompiler(CompilerOptions.EnableDiagnostics);

            var matched = compiler.CompileStepDefinitionElementFromStatementBody(LogFactory, StepType.Given, TestStep);

            matched.Output.Should().NotBeNull();

            var stepDefBuilder = new StepDefinitionBuilder(StepType.Given, "I have done something", 1, 1);
            stepDefBuilder.WordPart("I", 1);
            stepDefBuilder.WordPart("have", 3);
            stepDefBuilder.WordPart("done", 8);
            stepDefBuilder.WordPart("something", 13);

            AssertElementComparison(stepDefBuilder.Built, matched.Output, true);
        }

        [Fact]
        public void GenerateStepDefinitionWithArguments()
        {
            const string TestStep = "I have {argument1}";

            var compiler = new AutoStepCompiler(CompilerOptions.EnableDiagnostics);

            var matched = compiler.CompileStepDefinitionElementFromStatementBody(LogFactory, StepType.Given, TestStep);

            matched.Output.Should().NotBeNull();

            var stepDefinitionBuilder = new StepDefinitionBuilder(StepType.Given, TestStep, 1, 1)
                                            .WordPart("I", 1)
                                            .WordPart("have", 3)
                                            .Argument("{argument1}", "argument1", 8);

            AssertElementComparison(stepDefinitionBuilder.Built, matched.Output, true);
        }

        [Fact]
        public void EmptyArgumentCausesError()
        {
            const string TestStep = "I have done something with {}";

            var compiler = new AutoStepCompiler(CompilerOptions.EnableDiagnostics);

            var matched = compiler.CompileStepDefinitionElementFromStatementBody(LogFactory, StepType.Given, TestStep);

            matched.Success.Should().BeFalse();
            matched.Messages.Should().HaveCount(1);
            matched.Messages.Should().Contain(
                new CompilerMessage(null, CompilerMessageLevel.Error, CompilerMessageCode.StepVariableNameRequired,
                                    "You cannot specify an Empty Parameter as a Step Parameter. Step Parameter variables must be literal names, e.g. {variable1} or {total}.",
                                    1, 28, 1, 29
                                    ));
        }

        [Fact(Skip = "Future functionality; warn people when they have declared a step with an insertion variable")]
        public void InsertionArgumentCausesError()
        {
            const string TestStep = "I have done something with 'value <insert>'";

            var compiler = new AutoStepCompiler(CompilerOptions.EnableDiagnostics);

            var matched = compiler.CompileStepDefinitionElementFromStatementBody(LogFactory, StepType.Given, TestStep);

            matched.Success.Should().BeFalse();
            matched.Messages.Should().HaveCount(1);
            matched.Messages.Should().Contain(
                new CompilerMessage(null, CompilerMessageLevel.Error, CompilerMessageCode.CannotSpecifyDynamicValueInStepDefinition,
                                    "You cannot use 'value <insert>' as a Step Parameter. Step Parameter variables must be literal names, e.g. 'variable1' or 'total'. You cannot specify dynamic values.",
                                    1, 28, 1, 43
                                    ));
        }


        [Fact(Skip = "Future functionality; warn people when they have declared a step with an interpolation statement")]
        public void InterpolationArgument()
        {
            const string TestStep = "I have done something with ':interpolate'";

            var compiler = new AutoStepCompiler(CompilerOptions.EnableDiagnostics);

            var matched = compiler.CompileStepDefinitionElementFromStatementBody(LogFactory, StepType.Given, TestStep);

            matched.Success.Should().BeFalse();
            matched.Messages.Should().HaveCount(1);
            matched.Messages.Should().Contain(
                new CompilerMessage(null, CompilerMessageLevel.Error, CompilerMessageCode.CannotSpecifyDynamicValueInStepDefinition,
                                    "You cannot use ':interpolate' as a Step Parameter. Step Parameter variables must be literal names, e.g. 'variable1' or 'total'. You cannot specify dynamic values.",
                                    1, 28, 1, 41
                                    ));
        }
    }
}
