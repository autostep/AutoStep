using System;
using System.Linq;
using AutoStep.Language.Tests.DefinedSteps;
using AutoStep.Execution.Dependency;
using AutoStep.Tests.Utils;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;
using AutoStep.Definitions.Test;

namespace AutoStep.Tests.Definition
{
    public class AssemblyStepDefinitionSourceTests : CompilerTestBase
    {
        public AssemblyStepDefinitionSourceTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void CanLoadStepsFromAnAssembly()
        {
            var assemblySource = new AssemblyStepDefinitionSource(typeof(BasicSteps).Assembly, LogFactory);

            var stepDefinitions = assemblySource.GetStepDefinitions().ToList();

            stepDefinitions.Should().HaveCount(1);
            stepDefinitions[0].Type.Should().Be(StepType.Given);
            stepDefinitions[0].Declaration.Should().Be("I have clicked the button");
        }

        [Fact]
        public void RegistersConsumerClassesAsServices()
        {
            var assemblySource = new AssemblyStepDefinitionSource(typeof(BasicSteps).Assembly, LogFactory);

            var servicesBuilder = new AutofacServiceBuilder();

            assemblySource.ConfigureServices(servicesBuilder, new AutoStep.Execution.RunConfiguration());

            var resolve = servicesBuilder.BuildRootScope().Resolve<BasicSteps>();

            resolve.Should().NotBeNull();
        }

        [Fact]
        public void ConfigureServicesNullBuilderThrows()
        {
            var assemblySource = new AssemblyStepDefinitionSource(typeof(BasicSteps).Assembly, LogFactory);

            assemblySource.Invoking(a => a.ConfigureServices(null!, new AutoStep.Execution.RunConfiguration())).Should().Throw<ArgumentNullException>();
        }
    }
}
