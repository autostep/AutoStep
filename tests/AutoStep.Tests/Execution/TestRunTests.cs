using System;
using FluentAssertions;
using AutoStep.Execution;
using AutoStep.Projects;
using Moq;
using AutoStep.Language;
using Xunit;
using AutoStep.Tests.Utils;
using AutoStep.Tests.Builders;
using System.Threading.Tasks;
using AutoStep.Execution.Strategy;
using AutoStep.Execution.Dependency;
using Xunit.Abstractions;
using AutoStep.Execution.Contexts;
using AutoStep.Language.Interaction;
using Microsoft.Extensions.Configuration;
using System.Threading;
using Autofac;

namespace AutoStep.Tests.Execution
{
    public class TestRunTests : CompilerTestBase
    {
        private IConfiguration BlankConfiguration { get; } = new ConfigurationBuilder().Build();

        public TestRunTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void NullProjectArgumentException()
        {
            Action act = () => new TestRun(null!, BlankConfiguration);

            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void NullConfigArgumentAllowed()
        {
            var project = new Project();

            new TestRun(project, null!);
        }

        [Fact]
        public async Task ExecuteTestInvokesRunExecutionStrategy()
        {
            var builtFile = new FileBuilder().Feature("My Feature", 1, 1, feat => feat
                .Scenario("My Scenario", 1, 1)
            ).Built;

            var file = new ProjectTestFile("/path", new StringContentSource("test"));

            var mockInteractions = new Mock<IInteractionsConfiguration>();
            mockInteractions.Setup(c => c.RootMethodTable).Returns(new RootMethodTable());

            var mockProjectBuilder = new Mock<IProjectBuilder>();
            mockProjectBuilder.Setup(c => c.Interactions).Returns(mockInteractions.Object);

            file.SetFileReadyForRunTest(builtFile);

            var project = new Project(p => mockProjectBuilder.Object);
            project.TryAddFile(file);

            var testRun = new TestRun(project, BlankConfiguration);
            var runStrategyInvoked = false;

            var mockRunStrategy = new Mock<IRunExecutionStrategy>();
            mockRunStrategy.Setup(x => x.ExecuteAsync(It.IsAny<ILifetimeScope>(), It.IsAny<RunContext>(), It.IsAny<FeatureExecutionSet>(), CancellationToken.None))
                           .Callback((ILifetimeScope scope, RunContext ctxt, FeatureExecutionSet featureSet, CancellationToken cancelToken) =>
                           {
                               runStrategyInvoked = true;

                               scope.Should().NotBeNull();
                               ctxt.Should().NotBeNull();
                               featureSet.Should().NotBeNull();
                               featureSet.Features.Should().HaveCount(1);
                               featureSet.Features[0].Scenarios[0].Name.Should().Be("My Scenario");
                           });

            testRun.SetRunExecutionStrategy(mockRunStrategy.Object);

            var runResult = await testRun.ExecuteAsync(CancellationToken.None);

            runResult.Should().NotBeNull();
            runStrategyInvoked.Should().BeTrue();
        }

        [Fact]
        public async Task ExecuteTestInvokesRegisteredServiceConfigCallbacks()
        {
            var fakeProject = await ProjectMocks.CreateBuiltProject(("test", @"
                Feature: My Feature

                Scenario: Scenario 1

                    Then it fails
            "));

            var testRun = fakeProject.CreateTestRun();

            var wasInvoked = false;

            testRun.ConfigureContainer((cfg, srv) => { wasInvoked = true; });

            await testRun.ExecuteAsync(default);

            wasInvoked.Should().BeTrue();
        }
    }
}
