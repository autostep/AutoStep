using AutoStep.Execution;
using AutoStep.Execution.Contexts;
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
            var serviceBuilder = new AutofacServiceBuilder();

            serviceBuilder.RegisterPerResolveService<TestService>();

            var built = serviceBuilder.BuildRootScope();

            built.Resolve<TestService>();

            sharedCount.Should().Be(1);

            built.Resolve<TestService>();

            sharedCount.Should().Be(2);
        }

        [Fact]
        public void CanRegisterConsumerByType()
        {
            var serviceBuilder = new AutofacServiceBuilder();

            serviceBuilder.RegisterPerResolveService(typeof(TestService));

            var built = serviceBuilder.BuildRootScope();

            built.Resolve<TestService>();

            sharedCount.Should().Be(1);

            built.Resolve<TestService>();

            sharedCount.Should().Be(2);
        }

        [Fact]
        public void CanRegisterSingleInstance()
        {
            var serviceBuilder = new AutofacServiceBuilder();

            var instance = new TestService();

            sharedCount.Should().Be(1);

            serviceBuilder.RegisterSingleInstance(instance);

            using var built = serviceBuilder.BuildRootScope();

            built.Resolve<TestService>();

            sharedCount.Should().Be(1);

            built.Resolve<TestService>();

            sharedCount.Should().Be(1);

            using var nested = built.BeginNewScope(new RunContext(new RunConfiguration()));

            nested.Resolve<TestService>();

            sharedCount.Should().Be(1);
        }

        [Fact]
        public void CanRegisterPerFeatureService()
        {
            var serviceBuilder = new AutofacServiceBuilder();

            serviceBuilder.RegisterPerFeatureService<TestService>();

            using var built = serviceBuilder.BuildRootScope();

            built.Invoking(sc => sc.Resolve<TestService>()).Should().Throw<DependencyException>();

            using (var featureScope = built.BeginNewScope(ScopeTags.FeatureTag, new MyContext()))
            {
                featureScope.Resolve<TestService>().Should().NotBeNull();
                sharedCount.Should().Be(1);
                featureScope.Resolve<TestService>().Should().NotBeNull();
                sharedCount.Should().Be(1);

                using var scenarioScope = featureScope.BeginNewScope(ScopeTags.ScenarioTag, new MyContext());
                featureScope.Resolve<TestService>().Should().NotBeNull();
                sharedCount.Should().Be(1);
            }

            using (var featureScope = built.BeginNewScope(ScopeTags.FeatureTag, new MyContext()))
            {
                featureScope.Resolve<TestService>().Should().NotBeNull();
                sharedCount.Should().Be(2);
                featureScope.Resolve<TestService>().Should().NotBeNull();
                sharedCount.Should().Be(2);

                using var scenarioScope = featureScope.BeginNewScope(ScopeTags.ScenarioTag, new MyContext());
                featureScope.Resolve<TestService>().Should().NotBeNull();
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
            featureScope.Invoking(sc => sc.Resolve<TestService>()).Should().Throw<DependencyException>();

            using (var scenarioScope = featureScope.BeginNewScope(ScopeTags.ScenarioTag, new MyContext()))
            {
                scenarioScope.Resolve<TestService>().Should().NotBeNull();
                sharedCount.Should().Be(1);
                scenarioScope.Resolve<TestService>().Should().NotBeNull();
                sharedCount.Should().Be(1);
            }

            using (var scenarioScope = featureScope.BeginNewScope(ScopeTags.ScenarioTag, new MyContext()))
            {
                scenarioScope.Resolve<TestService>().Should().NotBeNull();
                sharedCount.Should().Be(2);
                scenarioScope.Resolve<TestService>().Should().NotBeNull();
                sharedCount.Should().Be(2);
            }
        }

        [Fact]
        public void CanRegisterPerScopeService()
        {
            var serviceBuilder = new AutofacServiceBuilder();

            serviceBuilder.RegisterPerScopeService<TestService>();

            using var built = serviceBuilder.BuildRootScope();

            using (var stepScope = built.BeginNewScope(new MyContext()))
            {
                stepScope.Resolve<TestService>().Should().NotBeNull();
                sharedCount.Should().Be(1);
                stepScope.Resolve<TestService>().Should().NotBeNull();
                sharedCount.Should().Be(1);
            }

            using (var stepScope = built.BeginNewScope(ScopeTags.GeneralScopeTag, new MyContext()))
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
