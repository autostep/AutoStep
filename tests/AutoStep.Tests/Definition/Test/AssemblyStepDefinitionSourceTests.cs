using System;
using System.Linq;
using AutoStep.Language.Tests.DefinedSteps;
using AutoStep.Execution.Dependency;
using AutoStep.Tests.Utils;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;
using AutoStep.Definitions.Test;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Autofac;

namespace AutoStep.Tests.Definition
{
    public class AssemblyStepDefinitionSourceTests : CompilerTestBase
    {
        private IConfiguration BlankConfiguration { get; } = new ConfigurationBuilder().Build();

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

            var servicesBuilder = new ContainerBuilder();

            assemblySource.ConfigureServices(servicesBuilder, BlankConfiguration);

            var resolve = servicesBuilder.Build().Resolve<BasicSteps>();

            resolve.Should().NotBeNull();
        }

        [Fact]
        public void ConfigureServicesNullBuilderThrows()
        {
            var assemblySource = new AssemblyStepDefinitionSource(typeof(BasicSteps).Assembly, LogFactory);

            assemblySource.Invoking(a => a.ConfigureServices(null!, BlankConfiguration)).Should().Throw<ArgumentNullException>();
        }
    }
}
