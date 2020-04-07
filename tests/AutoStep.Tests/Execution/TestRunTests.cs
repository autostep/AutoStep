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

            var mockProjectCompiler = new Mock<IProjectCompiler>();
            mockProjectCompiler.Setup(c => c.Interactions).Returns(mockInteractions.Object);

            file.SetFileReadyForRunTest(builtFile);

            var project = new Project(p => mockProjectCompiler.Object);
            project.TryAddFile(file);

            var testRun = new TestRun(project, BlankConfiguration);
            var runStrategyInvoked = false;

            var mockRunStrategy = new Mock<IRunExecutionStrategy>();
            mockRunStrategy.Setup(x => x.Execute(It.IsAny<IServiceScope>(), It.IsAny<RunContext>(), It.IsAny<FeatureExecutionSet>()))
                           .Callback((IServiceScope scope, RunContext ctxt, FeatureExecutionSet featureSet) =>
                           {
                               runStrategyInvoked = true;

                               scope.Should().NotBeNull();
                               ctxt.Should().NotBeNull();
                               featureSet.Should().NotBeNull();
                               featureSet.Features.Should().HaveCount(1);
                               featureSet.Features[0].Scenarios[0].Name.Should().Be("My Scenario");
                           });

            testRun.SetRunExecutionStrategy(mockRunStrategy.Object);

            var runResult = await testRun.ExecuteAsync(logCfg =>
            {

            });

            runResult.Should().NotBeNull();
            runStrategyInvoked.Should().BeTrue();
        }        
    }
}
