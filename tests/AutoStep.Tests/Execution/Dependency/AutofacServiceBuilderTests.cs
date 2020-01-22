using System;
using System.Collections.Generic;
using System.Text;
using Autofac;
using Autofac.Core;
using AutoStep.Execution;
using AutoStep.Execution.Dependency;
using FluentAssertions;
using Xunit;

namespace AutoStep.Tests.Execution.Dependency
{
    public class AutofacServiceBuilderTests
    {
        private static int sharedCount;

        public AutofacServiceBuilderTests()
        {
            sharedCount = 0;
        }

        [Fact]
        public void CanRegisterConsumer()
        {
            var container = new ContainerBuilder();

            var serviceBuilder = new AutofacServiceBuilder(container);

            serviceBuilder.RegisterConsumer<TestService>();

            var built = container.Build();

            built.Resolve<TestService>();

            sharedCount.Should().Be(1);

            built.Resolve<TestService>();

            sharedCount.Should().Be(2);
        }

        [Fact]
        public void CanRegisterConsumerByType()
        {
            var container = new ContainerBuilder();

            var serviceBuilder = new AutofacServiceBuilder(container);

            serviceBuilder.RegisterConsumer(typeof(TestService));

            var built = container.Build();

            built.Resolve<TestService>();

            sharedCount.Should().Be(1);

            built.Resolve<TestService>();

            sharedCount.Should().Be(2);
        }

        [Fact]
        public void CanRegisterSingleInstance()
        {
            var container = new ContainerBuilder();

            var serviceBuilder = new AutofacServiceBuilder(container);

            var instance = new TestService();

            sharedCount.Should().Be(1);

            serviceBuilder.RegisterSingleInstance(instance);

            using var built = container.Build();

            built.Resolve<TestService>();

            sharedCount.Should().Be(1);

            built.Resolve<TestService>();

            sharedCount.Should().Be(1);

            using var nested = built.BeginLifetimeScope();

            nested.Resolve<TestService>();

            sharedCount.Should().Be(1);
        }

        [Fact]
        public void CanRegisterPerFeatureService()
        {
            var container = new ContainerBuilder();

            var serviceBuilder = new AutofacServiceBuilder(container);

            serviceBuilder.RegisterPerFeatureService<TestService>();

            using var built = container.Build();
            
            built.Invoking(sc => sc.Resolve<TestService>()).Should().Throw<DependencyResolutionException>();

            using (var featureScope = built.BeginLifetimeScope(ScopeTags.FeatureTag))
            {
                featureScope.Resolve<TestService>().Should().NotBeNull();
                sharedCount.Should().Be(1);
                featureScope.Resolve<TestService>().Should().NotBeNull();
                sharedCount.Should().Be(1);

                using(var scenarioScope = featureScope.BeginLifetimeScope(ScopeTags.ScenarioTag))
                {
                    featureScope.Resolve<TestService>().Should().NotBeNull();
                    sharedCount.Should().Be(1);
                }
            }

            using (var featureScope = built.BeginLifetimeScope(ScopeTags.FeatureTag))
            {
                featureScope.Resolve<TestService>().Should().NotBeNull();
                sharedCount.Should().Be(2);
                featureScope.Resolve<TestService>().Should().NotBeNull();
                sharedCount.Should().Be(2);

                using (var scenarioScope = featureScope.BeginLifetimeScope(ScopeTags.ScenarioTag))
                {
                    featureScope.Resolve<TestService>().Should().NotBeNull();
                    sharedCount.Should().Be(2);
                }
            }
        }

        [Fact]
        public void CanRegisterPerScenarioService()
        {
            var container = new ContainerBuilder();

            var serviceBuilder = new AutofacServiceBuilder(container);

            serviceBuilder.RegisterPerScenarioService<TestService>();

            using var built = container.Build();

            using (var featureScope = built.BeginLifetimeScope(ScopeTags.FeatureTag))
            {
                featureScope.Invoking(sc => sc.Resolve<TestService>()).Should().Throw<DependencyResolutionException>();

                using (var scenarioScope = featureScope.BeginLifetimeScope(ScopeTags.ScenarioTag))
                {
                    scenarioScope.Resolve<TestService>().Should().NotBeNull();
                    sharedCount.Should().Be(1);
                    scenarioScope.Resolve<TestService>().Should().NotBeNull();
                    sharedCount.Should().Be(1);
                }

                using (var scenarioScope = featureScope.BeginLifetimeScope(ScopeTags.ScenarioTag))
                {
                    scenarioScope.Resolve<TestService>().Should().NotBeNull();
                    sharedCount.Should().Be(2);
                    scenarioScope.Resolve<TestService>().Should().NotBeNull();
                    sharedCount.Should().Be(2);
                }
            }
        }

        [Fact]
        public void CanRegisterPerScopeService()
        {
            var container = new ContainerBuilder();

            var serviceBuilder = new AutofacServiceBuilder(container);

            serviceBuilder.RegisterPerScopeService<TestService>();

            using var built = container.Build();

            using (var stepScope = built.BeginLifetimeScope())
            {
                stepScope.Resolve<TestService>().Should().NotBeNull();
                sharedCount.Should().Be(1);
                stepScope.Resolve<TestService>().Should().NotBeNull();
                sharedCount.Should().Be(1);
            }

            using (var stepScope = built.BeginLifetimeScope(ScopeTags.GeneralScopeTag))
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
    }
}
