using System.Linq;
using AutoStep.Compiler.Tests.DefinedSteps;
using AutoStep.Sources;
using AutoStep.Tests.Utils;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace AutoStep.Tests.Sources
{
    public class AssemblyStepDefinitionSourceTests : CompilerTestBase
    {
        public AssemblyStepDefinitionSourceTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void CanLoadStepsFromAnAssembly()
        {
            var assemblySource = new AssemblyStepDefinitionSource(typeof(BasicSteps).Assembly, TestTracer);

            var stepDefinitions = assemblySource.GetStepDefinitions().ToList();

            stepDefinitions.Should().HaveCount(1);
            stepDefinitions[0].Type.Should().Be(StepType.Given);
            stepDefinitions[0].Declaration.Should().Be("I have clicked the button");
        }
    }
}
