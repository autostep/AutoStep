using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using AutoStep.Language;
using AutoStep.Elements.Metadata;
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
        public async ValueTask SingleThreadedTest()
        {
            var features = Get2FeatureSet();
            var runContext = new RunContext(new RunConfiguration());
            var mockExecutionStateManager = new Mock<IExecutionStateManager>();
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

            await strategy.Execute(scope, runContext, features);

            beforeThread.Should().Be(1);
            afterThread.Should().Be(1);
            featureStrategy.AddedFeatures.Should().ContainInOrder(new[] { features.Features[0], features.Features[1] });
            mockExecutionStateManager.Verify(x => x.CheckforHalt(It.IsAny<IServiceScope>(), It.IsAny<ThreadContext>(), TestThreadState.Starting), Times.Once());
        }

        [Fact]
        public async ValueTask MultiThreadedTest()
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
            builder.RegisterSingleInstance<IEventPipeline>(eventPipeline);

            var scope = builder.BuildRootScope();

            var strategy = new DefaultRunExecutionStrategy();

            await strategy.Execute(scope, runContext, features);

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
        public async ValueTask DontUseMoreThreadsThanFeatures()
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
            builder.RegisterSingleInstance<IEventPipeline>(eventPipeline);

            var scope = builder.BuildRootScope();

            var strategy = new DefaultRunExecutionStrategy();

            await strategy.Execute(scope, runContext, features);

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
            var file1 = new ProjectTestFile("/path1", new StringContentSource("my file 1"));
            var file2 = new ProjectTestFile("/path2", new StringContentSource("my file 2"));

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
            public ConcurrentQueue<IFeatureInfo> AddedFeatures { get; } = new ConcurrentQueue<IFeatureInfo>();

            public ValueTask Execute(IServiceScope threadScope, ThreadContext threadContext, IFeatureInfo feature)
            {
                threadScope.Tag.Should().Be(ScopeTags.ThreadTag);

                AddedFeatures.Enqueue(feature);

                return default;
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

            public async ValueTask OnThread(IServiceScope scope, ThreadContext ctxt, Func<IServiceScope, ThreadContext, ValueTask> next)
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

            public ValueTask OnExecute(IServiceScope scope, RunContext ctxt, Func<IServiceScope, RunContext, ValueTask> next)
            {
                throw new NotImplementedException();
            }

            public void ConfigureServices(IServicesBuilder builder, RunConfiguration configuration)
            {
                throw new NotImplementedException();
            }

            public ValueTask OnFeature(IServiceScope scope, FeatureContext ctxt, Func<IServiceScope, FeatureContext, ValueTask> next)
            {
                throw new NotImplementedException();
            }

            public ValueTask OnScenario(IServiceScope scope, ScenarioContext ctxt, Func<IServiceScope, ScenarioContext, ValueTask> next)
            {
                throw new NotImplementedException();
            }

            public ValueTask OnStep(IServiceScope scope, StepContext ctxt, Func<IServiceScope, StepContext, ValueTask> next)
            {
                throw new NotImplementedException();
            }

        }
    }
}
