using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using AutoStep.Elements.Test;
using AutoStep.Execution;
using AutoStep.Execution.Contexts;
using AutoStep.Execution.Dependency;
using AutoStep.Execution.Events;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace AutoStep.Tests.Execution.Events
{
    public class EventPipelineTests
    {
        private IConfiguration BlankConfiguration { get; } = new ConfigurationBuilder().Build();

        [Fact]
        public async Task InvokeEventInvokesProvidedCallbackWithNoHandlers()
        {
            var pipeline = new EventPipeline(new List<IEventHandler>());
            var scopeMock = new Mock<ILifetimeScope>();
            var scope = scopeMock.Object;
            var context = new RunContext(BlankConfiguration);
            var callbackInvoked = false;
            var endOfPipelineInvoked = false;

            await pipeline.InvokeEventAsync(scope, context, (h, s, c, cancel, n) =>
            {
                callbackInvoked = true;

                return default;
            },
            CancellationToken.None,
            (sc, ctxt, cancel) =>
            {
                sc.Should().Be(scope);
                ctxt.Should().Be(context);
                endOfPipelineInvoked = true;

                return default;
            });

            callbackInvoked.Should().BeFalse();
            endOfPipelineInvoked.Should().BeTrue();
        }

        [Fact]
        public async Task InvokeEventCanCallHandlersWithNoTarget()
        {
            bool beforeCalled = false;
            bool afterCalled = false;

            var scopeMock = new Mock<ILifetimeScope>();
            var scope = scopeMock.Object;
            var context = new RunContext(BlankConfiguration);

            var myHandler = new MyEventHandler(() => beforeCalled = true, () => afterCalled = true);

            var pipeline = new EventPipeline(new List<IEventHandler> { myHandler });

            await pipeline.InvokeEventAsync(scope, context, (h, s, c, n, cancel) => h.OnExecuteAsync(s, c, n, cancel), CancellationToken.None);

            beforeCalled.Should().BeTrue();
            afterCalled.Should().BeTrue();
        }

        [Fact]
        public async Task AsyncTargetMethod()
        {
            var order = new List<int>();

            var scopeMock = new Mock<ILifetimeScope>();
            var scope = scopeMock.Object;
            var context = new RunContext(BlankConfiguration);

            var myHandler = new MyEventHandler(() => order.Add(1), () => order.Add(4));

            var pipeline = new EventPipeline(new List<IEventHandler> { myHandler });

            await pipeline.InvokeEventAsync(scope, context, (h, s, c, n, cancel) => h.OnExecuteAsync(s, c, n, cancel), CancellationToken.None, async (scope, ctxt, cancel) => {
                order.Add(2);
                await Task.Delay(10);
                order.Add(3);
            });

            order.Should().ContainInOrder(new[] { 1, 2, 3, 4 });
        }

        [Fact]
        public async Task EventHandlersInvokedInCorrectOrder()
        {
            var order = new List<int>();

            var scopeMock = new Mock<ILifetimeScope>();
            var scope = scopeMock.Object;
            var context = new RunContext(BlankConfiguration);

            var myHandler = new MyEventHandler(() => order.Add(1), () => order.Add(8));
            var myHandler2 = new AsyncEventHandler(() => order.Add(2), () => order.Add(7));
            var myHandler3 = new MyEventHandler(() => order.Add(3), () => order.Add(6));

            var pipeline = new EventPipeline(new List<IEventHandler> { myHandler, myHandler2, myHandler3 });

            await pipeline.InvokeEventAsync(scope, context, (h, s, c, n, cancel) => h.OnExecuteAsync(s, c, n, cancel), CancellationToken.None, async (scope, ctxt, cancel) => {
                order.Add(4);
                await Task.Delay(10);
                order.Add(5);
            });

            order.Should().ContainInOrder(new[] { 1, 2, 3, 4, 5, 6, 7, 8 });
        }

        [Fact]
        public async Task StepFailuresThrownAsIs()
        {
            var scopeMock = new Mock<ILifetimeScope>();
            var scope = scopeMock.Object;
            var context = new RunContext(BlankConfiguration);
            var mockStepReference = new StepReferenceElement();
            Exception? foundException = null;

            var myHandler = new MyEventHandler(ex => foundException = ex);

            var pipeline = new EventPipeline(new List<IEventHandler> { myHandler });

            await pipeline.InvokeEventAsync(scope, context, (h, s, c, n, cancel) => h.OnExecuteAsync(s, c, n, cancel), CancellationToken.None, (scope, ctxt, cancel) => {
                throw new StepFailureException(mockStepReference, new NullReferenceException());
            });

            foundException.Should().NotBeNull();
            foundException.Should().BeOfType<StepFailureException>();
            foundException!.InnerException.Should().BeOfType<NullReferenceException>();
        }

        [Fact]
        public async Task FailuresFromOtherEventHandlerThrownAsIs()
        {
            var scopeMock = new Mock<ILifetimeScope>();
            var scope = scopeMock.Object;
            var context = new RunContext(BlankConfiguration);
            Exception? foundException = null;

            var myHandler = new MyEventHandler(ex => foundException = ex);
            IEventHandler errHandler = null!;
            errHandler = new MyEventHandler(() => { }, () => throw new EventHandlingException(errHandler, new NullReferenceException()));

            var pipeline = new EventPipeline(new List<IEventHandler> { myHandler, errHandler });

            await pipeline.InvokeEventAsync(scope, context, (h, s, c, n, cancel) => h.OnExecuteAsync(s, c, n, cancel), CancellationToken.None);

            foundException.Should().NotBeNull();
            foundException.Should().BeOfType<EventHandlingException>();
            foundException!.InnerException.Should().BeOfType<NullReferenceException>();
        }

        [Fact]
        public void CancelledEventPipelineThrowsOperationCancelledAndPropagatesUp()
        {
            var scopeMock = new Mock<ILifetimeScope>();
            var scope = scopeMock.Object;
            var context = new RunContext(BlankConfiguration);
            var secondHandlerHit = false;
            var cancelSource = new CancellationTokenSource();
            
            // Cancel in the first handler.
            var firstHandler = new MyEventHandler(() => { cancelSource.Cancel(); }, () => { });
            var secondHandler = new MyEventHandler(() => { secondHandlerHit = true; }, () => { });

            var pipeline = new EventPipeline(new List<IEventHandler> { firstHandler, secondHandler });

            pipeline.Awaiting(ep => ep.InvokeEventAsync(scope, context, (h, s, c, n, cancel) => h.OnExecuteAsync(s, c, n, cancel), cancelSource.Token))
                    .Should().Throw<OperationCanceledException>();

            secondHandlerHit.Should().BeFalse();
        }

        [Fact]
        public async Task RemainingExceptionsWrappedAsEventHandlingException()
        {
            var scopeMock = new Mock<ILifetimeScope>();
            var scope = scopeMock.Object;
            var context = new RunContext(BlankConfiguration);
            Exception? foundException = null;

            var myHandler = new MyEventHandler(ex => foundException = ex);
            var errHandler = new MyEventHandler(() => { }, () => throw new NullReferenceException());

            var pipeline = new EventPipeline(new List<IEventHandler> { myHandler, errHandler });

            await pipeline.InvokeEventAsync(scope, context, (h, s, c, n, cancel) => h.OnExecuteAsync(s, c, n, cancel), CancellationToken.None);

            foundException.Should().NotBeNull();
            foundException.Should().BeOfType<EventHandlingException>();
            foundException!.InnerException.Should().BeOfType<NullReferenceException>();
        }

        private class MyEventHandler : BaseEventHandler
        {
            private readonly Action callBefore;
            private readonly Action callAfter;
            private readonly Action<Exception>? exception;

            public MyEventHandler(Action callBefore, Action callAfter, Action<Exception>? exception = null)
            {
                this.callBefore = callBefore;
                this.callAfter = callAfter;
                this.exception = exception;
            }

            public MyEventHandler(Action<Exception> exception)
            {
                this.callBefore = () => { };
                this.callAfter = () => { };
                this.exception = exception;
            }

            public override async ValueTask OnExecuteAsync(ILifetimeScope scope, RunContext ctxt, Func<ILifetimeScope, RunContext, CancellationToken, ValueTask> next, CancellationToken cancelToken)
            {
                callBefore();

                try
                {
                    // We're only going to use execute for testing; the method called has no bearing on the pipeline object.
                    await next(scope, ctxt, cancelToken);
                }
                catch(Exception ex)
                {
                    if (exception != null)
                    {
                        exception(ex);
                    }
                    else
                    {
                        throw;
                    }
                }

                callAfter();
            }
        }

        private class AsyncEventHandler : BaseEventHandler
        {
            private readonly Action callBefore;
            private readonly Action callAfter;

            public AsyncEventHandler(Action callBefore, Action callAfter)
            {
                this.callBefore = callBefore;
                this.callAfter = callAfter;
            }

            public override async ValueTask OnExecuteAsync(ILifetimeScope scope, RunContext ctxt, Func<ILifetimeScope, RunContext, CancellationToken, ValueTask> next, CancellationToken cancelToken)
            {
                callBefore();

                // We're only going to use execute for testing; the method called has no bearing on the pipeline object.
                await Task.Delay(10);

                await next(scope, ctxt, cancelToken);

                callAfter();
            }
        }
    }
}
