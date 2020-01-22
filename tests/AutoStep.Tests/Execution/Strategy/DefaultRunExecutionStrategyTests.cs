using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using AutoStep.Compiler;
using AutoStep.Elements.ReadOnly;
using AutoStep.Execution;
using AutoStep.Execution.Contexts;
using AutoStep.Execution.Control;
using AutoStep.Execution.Dependency;
using AutoStep.Execution.Events;
using AutoStep.Execution.Strategy;
using AutoStep.Projects;
using AutoStep.Tests.Builders;
using AutoStep.Tests.Utils;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace AutoStep.Tests.Execution.Strategy
{
    public class DefaultRunExecutionStrategyTests : LoggingTestBase
    {
        public DefaultRunExecutionStrategyTests(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }

        [Fact]
        public async Task SingleThreadedTest()
        {
            var features = Get2FeatureSet();
            var runContext = new RunContext(new RunConfiguration());
            var mockExecutionStateManager = new Mock<IExecutionStateManager>();
            mockExecutionStateManager.Setup(x => x.CheckforHalt(It.IsAny<IServiceScope>(), It.IsAny<ThreadContext>(), TestThreadState.Starting)).Verifiable();
            var beforeThread = 0;
            var afterThread = 0;
            var eventHandler = new MyEventHandler((ThreadContext ctxt) =>
            {
                ctxt.TestThreadId.Should().Be(1);
                beforeThread++;
            }, c => afterThread++);
            
            var eventPipeline = new EventPipeline(new List<IEventHandler> { eventHandler });
            var featureStrategy = new MyFeatureStrategy();

            var builder = new AutofacServiceBuilder();

            builder.RegisterSingleInstance(LogFactory);
            builder.RegisterSingleInstance(mockExecutionStateManager.Object);
            builder.RegisterSingleInstance<IFeatureExecutionStrategy>(featureStrategy);  

            var scope = builder.BuildRootScope();

            var strategy = new DefaultRunExecutionStrategy();

            await strategy.Execute(scope, runContext, features, eventPipeline);

            featureStrategy.AddedFeatures.Should().ContainInOrder(new[] { features.Features[0], features.Features[1] });
            beforeThread.Should().Be(1);
            afterThread.Should().Be(1);
            mockExecutionStateManager.Verify(x => x.CheckforHalt(It.IsAny<IServiceScope>(), It.IsAny<ThreadContext>(), TestThreadState.Starting), Times.Once());
        }

        [Fact]
        public async Task MultiThreadedTest()
        {
            var features = Get2FeatureSet();
            var runContext = new RunContext(new RunConfiguration { ParallelCount = 2 });
            var mockExecutionStateManager = new Mock<IExecutionStateManager>();
            mockExecutionStateManager.Setup(x => x.CheckforHalt(It.IsAny<IServiceScope>(), It.IsAny<ThreadContext>(), TestThreadState.Starting)).Verifiable();

            var beforeThread = 0;
            var afterThread = 0;
            var threadBag = new ConcurrentBag<int>();
            var eventHandler = new MyEventHandler((ThreadContext ctxt) =>
            {
                Interlocked.Increment(ref beforeThread);
                threadBag.Add(ctxt.TestThreadId);
            }, c => Interlocked.Increment(ref afterThread));

            var eventPipeline = new EventPipeline(new List<IEventHandler> { eventHandler });
            var featureStrategy = new MyFeatureStrategy();

            var builder = new AutofacServiceBuilder();

            builder.RegisterSingleInstance(LogFactory);
            builder.RegisterSingleInstance(mockExecutionStateManager.Object);
            builder.RegisterSingleInstance<IFeatureExecutionStrategy>(featureStrategy);

            var scope = builder.BuildRootScope();

            var strategy = new DefaultRunExecutionStrategy();

            await strategy.Execute(scope, runContext, features, eventPipeline);

            featureStrategy.AddedFeatures.Should().Contain(features.Features[0]);
            featureStrategy.AddedFeatures.Should().Contain(features.Features[1]);
            threadBag.Should().Contain(new[] { 1, 2 });
            beforeThread.Should().Be(2);
            afterThread.Should().Be(2);
            mockExecutionStateManager.Verify(x => x.CheckforHalt(It.IsAny<IServiceScope>(), It.IsAny<ThreadContext>(), TestThreadState.Starting), Times.Exactly(2));

            LoggedMessages.ShouldContain(LogLevel.Debug, "executing", "Feature 1");
            LoggedMessages.ShouldContain(LogLevel.Debug, "executing", "Feature 2");
            LoggedMessages.ShouldContain(LogLevel.Debug, "Test Thread ID 1", "no more features");
            LoggedMessages.ShouldContain(LogLevel.Debug, "Test Thread ID 2", "no more features");
        }


        [Fact]
        public async Task DontUseMoreThreadsThanFeatures()
        {
            var features = Get2FeatureSet();
            // Say to use 3 threads, but we should still only use 2.
            var runContext = new RunContext(new RunConfiguration { ParallelCount = 3 });
            var mockExecutionStateManager = new Mock<IExecutionStateManager>();
            mockExecutionStateManager.Setup(x => x.CheckforHalt(It.IsAny<IServiceScope>(), It.IsAny<ThreadContext>(), TestThreadState.Starting)).Verifiable();

            var beforeThread = 0;
            var afterThread = 0;
            var threadBag = new ConcurrentBag<int>();
            var eventHandler = new MyEventHandler((ThreadContext ctxt) =>
            {
                Interlocked.Increment(ref beforeThread);
                threadBag.Add(ctxt.TestThreadId);
            }, c => Interlocked.Increment(ref afterThread));

            var eventPipeline = new EventPipeline(new List<IEventHandler> { eventHandler });
            var featureStrategy = new MyFeatureStrategy();

            var builder = new AutofacServiceBuilder();

            builder.RegisterSingleInstance(LogFactory);
            builder.RegisterSingleInstance(mockExecutionStateManager.Object);
            builder.RegisterSingleInstance<IFeatureExecutionStrategy>(featureStrategy);

            var scope = builder.BuildRootScope();

            var strategy = new DefaultRunExecutionStrategy();

            await strategy.Execute(scope, runContext, features, eventPipeline);

            featureStrategy.AddedFeatures.Should().Contain(features.Features[0]);
            featureStrategy.AddedFeatures.Should().Contain(features.Features[1]);

            threadBag.Should().Contain(new[] { 1, 2 });
            beforeThread.Should().Be(2);
            afterThread.Should().Be(2);
            mockExecutionStateManager.Verify(x => x.CheckforHalt(It.IsAny<IServiceScope>(), It.IsAny<ThreadContext>(), TestThreadState.Starting), Times.Exactly(2));
        }

        private FeatureExecutionSet Get2FeatureSet()
        {
            var project = new Project();
            var file1 = new ProjectFile("/path1", new StringContentSource("my file 1"));
            var file2 = new ProjectFile("/path2", new StringContentSource("my file 2"));

            project.TryAddFile(file1);
            project.TryAddFile(file2);

            var builtFile1 = new FileBuilder().Feature("My Feature 1", 1, 1, feat => feat
                .Scenario("My Scenario 1", 1, 1)
            ).Built;

            var builtFile2 = new FileBuilder().Feature("My Feature 2", 1, 1, feat => feat
                .Scenario("My Scenario 2", 1, 1)
            ).Built;

            file1.SetFileReadyForRunTest(builtFile1);
            file2.SetFileReadyForRunTest(builtFile2);

            return FeatureExecutionSet.Create(project, new RunAllFilter(), LogFactory);
        }

        private class MyFeatureStrategy : IFeatureExecutionStrategy
        {
            public List<IFeatureInfo> AddedFeatures { get; } = new List<IFeatureInfo>();

            public Task Execute(IServiceScope threadScope, IEventPipeline events, IFeatureInfo feature)
            {
                threadScope.Tag.Should().Be(ScopeTags.ThreadTag);

                AddedFeatures.Add(feature);

                return Task.CompletedTask;
            }
        }


        private class MyEventHandler : IEventHandler
        {
            private readonly Action<ThreadContext> callBefore;
            private readonly Action<ThreadContext> callAfter;
            private readonly Action<Exception> exception;

            public MyEventHandler(Action<ThreadContext> callBefore, Action<ThreadContext> callAfter, Action<Exception> exception = null)
            {
                this.callBefore = callBefore;
                this.callAfter = callAfter;
                this.exception = exception;
            }

            public MyEventHandler(Action<Exception> exception)
            {
                this.callBefore = c => { };
                this.callAfter = c => { };
                this.exception = exception;
            }

            public async Task Thread(IServiceScope scope, ThreadContext ctxt, Func<IServiceScope, ThreadContext, Task> next)
            {
                callBefore(ctxt);

                try
                {
                    await next(scope, ctxt);
                }
                catch (Exception ex)
                {
                    if (exception is object)
                    {
                        exception(ex);
                    }
                    else
                    {
                        throw;
                    }
                }

                callAfter(ctxt);
            }

            public Task Execute(IServiceScope scope, RunContext ctxt, Func<IServiceScope, RunContext, Task> next)
            {
                throw new NotImplementedException();
            }

            public void ConfigureServices(IServicesBuilder builder, RunConfiguration configuration)
            {
                throw new NotImplementedException();
            }

            public Task Feature(IServiceScope scope, FeatureContext ctxt, Func<IServiceScope, FeatureContext, Task> next)
            {
                throw new NotImplementedException();
            }

            public Task Scenario(IServiceScope scope, ScenarioContext ctxt, Func<IServiceScope, ScenarioContext, Task> next)
            {
                throw new NotImplementedException();
            }

            public Task Step(IServiceScope scope, StepContext ctxt, Func<IServiceScope, StepContext, Task> next)
            {
                throw new NotImplementedException();
            }

        }
    }
}
