using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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
using Microsoft.Extensions.Configuration;
using System.Linq;
using AutoStep.Execution.Logging;
using Autofac;

namespace AutoStep.Tests.Execution.Strategy
{
    public class DefaultRunExecutionStrategyTests : LoggingTestBase
    {
        private IConfiguration BlankConfiguration { get; } = new ConfigurationBuilder().Build();

        public DefaultRunExecutionStrategyTests(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }

        [Fact]
        public async Task SingleThreadedTest()
        {
            var features = Get2FeatureSet();
            var runContext = new RunContext(BlankConfiguration);
            var mockExecutionStateManager = new Mock<IExecutionStateManager>();
            var beforeThread = 0;
            var afterThread = 0;
            var eventHandler = new MyEventHandler(ctxt =>
            {
                ctxt.TestThreadId.Should().Be(1);
                beforeThread++;
            }, c => afterThread++);

            var featureStrategy = new MyFeatureStrategy();

            var eventPipeline = new EventPipeline(new List<IEventHandler> { eventHandler });

            var contextScopeDisposeTracker = new TestDisposable();

            var contextProvider = new Mock<IContextScopeProvider>();
            contextProvider.Setup(x => x.EnterContextScope(It.IsAny<TestExecutionContext>())).Returns<TestExecutionContext>(ctxt =>
            {
                ctxt.Should().BeOfType<ThreadContext>();

                return contextScopeDisposeTracker;
            });

            var builder = new ContainerBuilder();
            builder.RegisterInstance(LogFactory);
            builder.RegisterGeneric(typeof(Logger<>)).As(typeof(ILogger<>));
            builder.RegisterInstance(mockExecutionStateManager.Object);
            builder.RegisterInstance<IFeatureExecutionStrategy>(featureStrategy);
            builder.RegisterInstance<IEventPipeline>(eventPipeline);
            builder.RegisterInstance(contextProvider.Object);

            var scope = builder.Build();

            var strategy = new DefaultRunExecutionStrategy();

            await strategy.ExecuteAsync(scope, runContext, features, CancellationToken.None);

            beforeThread.Should().Be(1);
            afterThread.Should().Be(1);
            featureStrategy.AddedFeatures.Should().ContainInOrder(new[] { features.Features[0], features.Features[1] });
            mockExecutionStateManager.Verify(x => x.CheckforHalt(It.IsAny<ILifetimeScope>(), It.IsAny<ThreadContext>(), TestThreadState.Starting), Times.Once());
            contextScopeDisposeTracker.IsDisposed.Should().BeTrue();
        }

        [Fact]
        public async Task MultiThreadedTest()
        {
            var features = Get2FeatureSet();
            var runContext = new RunContext(GetTestConfig(("parallelCount", "2")));
            var mockExecutionStateManager = new Mock<IExecutionStateManager>();
            mockExecutionStateManager.Setup(x => x.CheckforHalt(It.IsAny<ILifetimeScope>(), It.IsAny<ThreadContext>(), TestThreadState.Starting)).Verifiable();

            var beforeThread = 0;
            var afterThread = 0;
            var threadBag = new ConcurrentBag<int>();
            var eventHandler = new MyEventHandler(ctxt =>
            {
                Interlocked.Increment(ref beforeThread);
                threadBag.Add(ctxt.TestThreadId);
            }, c => Interlocked.Increment(ref afterThread));

            var eventPipeline = new EventPipeline(new List<IEventHandler> { eventHandler });
            var featureStrategy = new MyFeatureStrategy();

            var contextScopeDisposeTracker = new TestDisposable();

            var contextProvider = new Mock<IContextScopeProvider>();
            contextProvider.Setup(x => x.EnterContextScope(It.IsAny<TestExecutionContext>())).Returns<TestExecutionContext>(ctxt =>
            {
                ctxt.Should().BeOfType<ThreadContext>();

                return contextScopeDisposeTracker;
            });

            var builder = new ContainerBuilder();
            builder.RegisterInstance(LogFactory);
            builder.RegisterGeneric(typeof(Logger<>)).As(typeof(ILogger<>));
            builder.RegisterInstance(mockExecutionStateManager.Object);
            builder.RegisterInstance<IFeatureExecutionStrategy>(featureStrategy);
            builder.RegisterInstance<IEventPipeline>(eventPipeline);
            builder.RegisterInstance(contextProvider.Object);

            var scope = builder.Build();

            var strategy = new DefaultRunExecutionStrategy();

            await strategy.ExecuteAsync(scope, runContext, features, CancellationToken.None);

            featureStrategy.AddedFeatures.Should().Contain(features.Features[0]);
            featureStrategy.AddedFeatures.Should().Contain(features.Features[1]);
            threadBag.Should().Contain(new[] { 1, 2 });
            beforeThread.Should().Be(2);
            afterThread.Should().Be(2);
            mockExecutionStateManager.Verify(x => x.CheckforHalt(It.IsAny<ILifetimeScope>(), It.IsAny<ThreadContext>(), TestThreadState.Starting), Times.Exactly(2));
            contextScopeDisposeTracker.IsDisposed.Should().BeTrue();

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
            var runContext = new RunContext(GetTestConfig(("parallelCount", "3")));
            var mockExecutionStateManager = new Mock<IExecutionStateManager>();
            mockExecutionStateManager.Setup(x => x.CheckforHalt(It.IsAny<ILifetimeScope>(), It.IsAny<ThreadContext>(), TestThreadState.Starting)).Verifiable();

            var beforeThread = 0;
            var afterThread = 0;
            var threadBag = new ConcurrentBag<int>();
            var eventHandler = new MyEventHandler(ctxt =>
            {
                Interlocked.Increment(ref beforeThread);
                threadBag.Add(ctxt.TestThreadId);
            }, c => Interlocked.Increment(ref afterThread));

            var eventPipeline = new EventPipeline(new List<IEventHandler> { eventHandler });
            var featureStrategy = new MyFeatureStrategy();


            var contextScopeDisposeTracker = new TestDisposable();

            var contextProvider = new Mock<IContextScopeProvider>();
            contextProvider.Setup(x => x.EnterContextScope(It.IsAny<TestExecutionContext>())).Returns<TestExecutionContext>(ctxt =>
            {
                ctxt.Should().BeOfType<ThreadContext>();

                return contextScopeDisposeTracker;
            });

            var builder = new ContainerBuilder();
            builder.RegisterInstance(mockExecutionStateManager.Object);
            builder.RegisterInstance<IFeatureExecutionStrategy>(featureStrategy);
            builder.RegisterInstance<IEventPipeline>(eventPipeline);
            builder.RegisterInstance(contextProvider.Object);
            builder.RegisterInstance(LogFactory);
            builder.RegisterGeneric(typeof(Logger<>)).As(typeof(ILogger<>));

            var scope = builder.Build();

            var strategy = new DefaultRunExecutionStrategy();

            await strategy.ExecuteAsync(scope, runContext, features, CancellationToken.None);

            featureStrategy.AddedFeatures.Should().Contain(features.Features[0]);
            featureStrategy.AddedFeatures.Should().Contain(features.Features[1]);

            threadBag.Should().Contain(new[] { 1, 2 });
            beforeThread.Should().Be(2);
            afterThread.Should().Be(2);
            mockExecutionStateManager.Verify(x => x.CheckforHalt(It.IsAny<ILifetimeScope>(), It.IsAny<ThreadContext>(), TestThreadState.Starting), Times.Exactly(2));
            contextScopeDisposeTracker.IsDisposed.Should().BeTrue();
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

        private IConfiguration GetTestConfig(params (string Key, string Value)[] values)
        {
            return new ConfigurationBuilder().AddInMemoryCollection(values.Select(item => new KeyValuePair<string, string>(item.Key, item.Value))).Build();
        }

        private class MyFeatureStrategy : IFeatureExecutionStrategy
        {
            public ConcurrentQueue<IFeatureInfo> AddedFeatures { get; } = new ConcurrentQueue<IFeatureInfo>();

            public ValueTask ExecuteAsync(ILifetimeScope threadScope, ThreadContext threadContext, IFeatureInfo feature, CancellationToken cancelToken)
            {
                threadScope.Tag.Should().Be(ScopeTags.ThreadTag);

                AddedFeatures.Enqueue(feature);

                return default;
            }
        }

        private class MyEventHandler : BaseEventHandler
        {
            private readonly Action<ThreadContext> callBefore;
            private readonly Action<ThreadContext> callAfter;
            private readonly Action<Exception>? exception;

            public MyEventHandler(Action<ThreadContext> callBefore, Action<ThreadContext> callAfter, Action<Exception>? exception = null)
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

            public override async ValueTask OnThreadAsync(ILifetimeScope scope, ThreadContext ctxt, Func<ILifetimeScope, ThreadContext, CancellationToken, ValueTask> next, CancellationToken cancelToken)
            {
                callBefore(ctxt);

                try
                {
                    await next(scope, ctxt, cancelToken);
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
        }
    }
}
