using Autofac;
using Autofac.Core;
using AutoStep.Execution;
using AutoStep.Execution.Contexts;
using AutoStep.Execution.Dependency;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace AutoStep.Tests.Execution.Dependency
{
    public class RegistrationExtensionTests
    {
        private static int sharedCount;

        public RegistrationExtensionTests()
        {
            sharedCount = 0;
        }

        [Fact]
        public void CanRegisterPerFeatureService()
        {
            var serviceBuilder = new ContainerBuilder();

            serviceBuilder.RegisterType<TestService>().InstancePerFeature();

            using var built = serviceBuilder.Build();

            built.Invoking(sc => sc.Resolve<TestService>()).Should().Throw<DependencyResolutionException>();

            using (var featureScope = built.BeginContextScope(ScopeTags.FeatureTag, new MyContext()))
            {
                featureScope.Resolve<TestService>().Should().NotBeNull();
                sharedCount.Should().Be(1);
                featureScope.Resolve<TestService>().Should().NotBeNull();
                sharedCount.Should().Be(1);

                using var scenarioScope = featureScope.BeginContextScope(ScopeTags.ScenarioTag, new MyContext());
                scenarioScope.Resolve<TestService>().Should().NotBeNull();
                sharedCount.Should().Be(1);
            }

            using (var featureScope = built.BeginContextScope(ScopeTags.FeatureTag, new MyContext()))
            {
                featureScope.Resolve<TestService>().Should().NotBeNull();
                sharedCount.Should().Be(2);
                featureScope.Resolve<TestService>().Should().NotBeNull();
                sharedCount.Should().Be(2);

                using var scenarioScope = featureScope.BeginContextScope(ScopeTags.ScenarioTag, new MyContext());
                scenarioScope.Resolve<TestService>().Should().NotBeNull();
                sharedCount.Should().Be(2);
            }
        }

        [Fact]
        public void CanRegisterPerScenarioService()
        {
            var serviceBuilder = new ContainerBuilder();

            serviceBuilder.RegisterType<TestService>().InstancePerScenario();

            using var built = serviceBuilder.Build();

            using var featureScope = built.BeginContextScope(ScopeTags.FeatureTag, new MyContext());
            featureScope.Invoking(sc => sc.Resolve<TestService>()).Should().Throw<DependencyResolutionException>();

            using (var scenarioScope = featureScope.BeginContextScope(ScopeTags.ScenarioTag, new MyContext()))
            {
                scenarioScope.Resolve<TestService>().Should().NotBeNull();
                sharedCount.Should().Be(1);
                scenarioScope.Resolve<TestService>().Should().NotBeNull();
                sharedCount.Should().Be(1);
            }

            using (var scenarioScope = featureScope.BeginContextScope(ScopeTags.ScenarioTag, new MyContext()))
            {
                scenarioScope.Resolve<TestService>().Should().NotBeNull();
                sharedCount.Should().Be(2);
                scenarioScope.Resolve<TestService>().Should().NotBeNull();
                sharedCount.Should().Be(2);
            }
        }

        [Fact]
        public void CanRegisterPerStepService()
        {
            var serviceBuilder = new ContainerBuilder();

            serviceBuilder.RegisterType<TestService>().InstancePerStep();

            using var built = serviceBuilder.Build();

            using (var stepScope = built.BeginContextScope(ScopeTags.StepTag, new MyContext()))
            {
                stepScope.Resolve<TestService>().Should().NotBeNull();
                sharedCount.Should().Be(1);
                stepScope.Resolve<TestService>().Should().NotBeNull();
                sharedCount.Should().Be(1);
            }

            using (var stepScope = built.BeginContextScope(ScopeTags.StepTag, new MyContext()))
            {
                stepScope.Resolve<TestService>().Should().NotBeNull();
                sharedCount.Should().Be(2);
                stepScope.Resolve<TestService>().Should().NotBeNull();
                sharedCount.Should().Be(2);
            }
        }

        private class TestService
        {
            public TestService()
            {
                sharedCount++;
            }
        }

        private class MyContext : TestExecutionContext
        {
        }
    }
}
