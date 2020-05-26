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
            var serviceBuilder = new AutofacServiceBuilder();

            serviceBuilder.RegisterPerResolveService<TestService>();

            var built = serviceBuilder.BuildRootScope();

            built.GetRequiredService<TestService>();

            sharedCount.Should().Be(1);

            built.GetRequiredService<TestService>();

            sharedCount.Should().Be(2);
        }

        [Fact]
        public void CanRegisterConsumerByType()
        {
            var serviceBuilder = new AutofacServiceBuilder();

            serviceBuilder.RegisterPerResolveService(typeof(TestService));

            var built = serviceBuilder.BuildRootScope();

            built.GetRequiredService<TestService>();

            sharedCount.Should().Be(1);

            built.GetRequiredService<TestService>();

            sharedCount.Should().Be(2);
        }

        [Fact]
        public void CanRegisterSingleInstance()
        {
            var serviceBuilder = new AutofacServiceBuilder();

            var instance = new TestService();

            sharedCount.Should().Be(1);

            serviceBuilder.RegisterInstance(instance);

            using var built = serviceBuilder.BuildRootScope();

            built.GetRequiredService<TestService>();

            sharedCount.Should().Be(1);

            built.GetRequiredService<TestService>();

            sharedCount.Should().Be(1);

            using var nested = built.BeginNewScope(ScopeTags.RunTag, new RunContext(new ConfigurationBuilder().Build()));

            nested.GetRequiredService<TestService>();

            sharedCount.Should().Be(1);
        }

        [Fact]
        public void CanRegisterPerFeatureService()
        {
            var serviceBuilder = new AutofacServiceBuilder();

            serviceBuilder.RegisterPerFeatureService<TestService>();

            using var built = serviceBuilder.BuildRootScope();

            built.Invoking(sc => sc.GetRequiredService<TestService>()).Should().Throw<DependencyException>();

            using (var featureScope = built.BeginNewScope(ScopeTags.FeatureTag, new MyContext()))
            {
                featureScope.GetRequiredService<TestService>().Should().NotBeNull();
                sharedCount.Should().Be(1);
                featureScope.GetRequiredService<TestService>().Should().NotBeNull();
                sharedCount.Should().Be(1);

                using var scenarioScope = featureScope.BeginNewScope(ScopeTags.ScenarioTag, new MyContext());
                featureScope.GetRequiredService<TestService>().Should().NotBeNull();
                sharedCount.Should().Be(1);
            }

            using (var featureScope = built.BeginNewScope(ScopeTags.FeatureTag, new MyContext()))
            {
                featureScope.GetRequiredService<TestService>().Should().NotBeNull();
                sharedCount.Should().Be(2);
                featureScope.GetRequiredService<TestService>().Should().NotBeNull();
                sharedCount.Should().Be(2);

                using var scenarioScope = featureScope.BeginNewScope(ScopeTags.ScenarioTag, new MyContext());
                featureScope.GetRequiredService<TestService>().Should().NotBeNull();
                sharedCount.Should().Be(2);
            }
        }

        [Fact]
        public void CanRegisterPerScenarioService()
        {
            var serviceBuilder = new AutofacServiceBuilder();

            serviceBuilder.RegisterPerScenarioService<TestService>();

            using var built = serviceBuilder.BuildRootScope();

            using var featureScope = built.BeginNewScope(ScopeTags.FeatureTag, new MyContext());
            featureScope.Invoking(sc => sc.GetRequiredService<TestService>()).Should().Throw<DependencyException>();

            using (var scenarioScope = featureScope.BeginNewScope(ScopeTags.ScenarioTag, new MyContext()))
            {
                scenarioScope.GetRequiredService<TestService>().Should().NotBeNull();
                sharedCount.Should().Be(1);
                scenarioScope.GetRequiredService<TestService>().Should().NotBeNull();
                sharedCount.Should().Be(1);
            }

            using (var scenarioScope = featureScope.BeginNewScope(ScopeTags.ScenarioTag, new MyContext()))
            {
                scenarioScope.GetRequiredService<TestService>().Should().NotBeNull();
                sharedCount.Should().Be(2);
                scenarioScope.GetRequiredService<TestService>().Should().NotBeNull();
                sharedCount.Should().Be(2);
            }
        }

        [Fact]
        public void CanRegisterPerStepService()
        {
            var serviceBuilder = new AutofacServiceBuilder();

            serviceBuilder.RegisterPerStepService<TestService>();

            using var built = serviceBuilder.BuildRootScope();

            using (var stepScope = built.BeginNewScope(ScopeTags.StepTag, new MyContext()))
            {
                stepScope.GetRequiredService<TestService>().Should().NotBeNull();
                sharedCount.Should().Be(1);
                stepScope.GetRequiredService<TestService>().Should().NotBeNull();
                sharedCount.Should().Be(1);
            }

            using (var stepScope = built.BeginNewScope(ScopeTags.StepTag, new MyContext()))
            {
                stepScope.GetRequiredService<TestService>().Should().NotBeNull();
                sharedCount.Should().Be(2);
                stepScope.GetRequiredService<TestService>().Should().NotBeNull();
                sharedCount.Should().Be(2);
            }
        }

        [Fact]
        public void CanResolveOpenLogger()
        {
            var serviceBuilder = new AutofacServiceBuilder();

            serviceBuilder.ConfigureLogging(new NullLoggerFactory());

            var built = serviceBuilder.BuildRootScope();

            var logger = built.GetRequiredService<ILogger<AutofacServiceBuilderTests>>();

            logger.Should().NotBeNull();
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
