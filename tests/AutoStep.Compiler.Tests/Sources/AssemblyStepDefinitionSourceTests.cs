using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoStep.Compiler.Tests.DefinedSteps;
using AutoStep.Compiler.Tests.Utils;
using AutoStep.Core;
using AutoStep.Core.Sources;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace AutoStep.Compiler.Tests.Sources
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
