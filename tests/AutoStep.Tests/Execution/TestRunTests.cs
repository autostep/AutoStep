﻿using System;
using FluentAssertions;
using AutoStep.Execution;
using AutoStep.Projects;
using Moq;
using AutoStep.Compiler;
using Xunit;
using AutoStep.Tests.Utils;
using AutoStep.Tests.Builders;
using System.Linq;
using System.Threading.Tasks;
using AutoStep.Execution.Strategy;
using AutoStep.Execution.Dependency;
using Xunit.Abstractions;
using AutoStep.Execution.Events;

namespace AutoStep.Tests.Execution
{
    public class TestRunTests : CompilerTestBase
    {
        public TestRunTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void NullProjectArgumentException()
        {
            var project = new Project();
            
            Action act = () => new TestRun(null, new RunConfiguration(), TestLogFactory.CreateNull());

            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void NullConfigArgumentException()
        {
            var project = new Project();

            Action act = () => new TestRun(project, null, TestLogFactory.CreateNull());

            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public async Task ExecuteTestInvokesRunExecutionStrategy()
        {
            var builtFile = new FileBuilder().Feature("My Feature", 1, 1, feat => feat
                .Scenario("My Scenario", 1, 1)
            ).Built;

            var file = new ProjectFile("/path", new StringContentSource("test"));

            var mockProjectCompiler = new Mock<IProjectCompiler>();

            file.SetFileReadyForRunTest(builtFile);

            var project = new Project(mockProjectCompiler.Object);
            project.TryAddFile(file);

            var testRun = new TestRun(project, new RunConfiguration(), LogFactory);
            var runStrategyInvoked = false;

            var mockRunStrategy = new Mock<IRunExecutionStrategy>();
            mockRunStrategy.Setup(x => x.Execute(It.IsAny<IServiceScope>(), It.IsAny<RunContext>(), It.IsAny<FeatureExecutionSet>(), It.IsAny<IEventPipeline>()))                
                           .Callback((IServiceScope scope, RunContext ctxt, FeatureExecutionSet featureSet, IEventPipeline pipeline) =>
                           {
                               runStrategyInvoked = true;

                               scope.Should().NotBeNull();
                               ctxt.Should().NotBeNull();
                               featureSet.Should().NotBeNull();
                               featureSet.Features.Should().HaveCount(1);
                               featureSet.Features[0].Scenarios[0].Name.Should().Be("My Scenario");
                               pipeline.Should().NotBeNull();
                           });

            testRun.SetRunExecutionStrategy(mockRunStrategy.Object);

            var runResult = await testRun.Execute();

            runResult.Should().NotBeNull();
            runStrategyInvoked.Should().BeTrue();
        }
    }
}