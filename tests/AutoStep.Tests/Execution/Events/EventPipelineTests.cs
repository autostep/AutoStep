using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoStep.Elements.Test;
using AutoStep.Execution;
using AutoStep.Execution.Contexts;
using AutoStep.Execution.Dependency;
using AutoStep.Execution.Events;
using FluentAssertions;
using Moq;
using Xunit;

namespace AutoStep.Tests.Execution.Events
{
    public class EventPipelineTests
    {
        [Fact]
        public void InvokeEventInvokesProvidedCallbackWithNoHandlers()
        {
            var pipeline = new EventPipeline(new List<IEventHandler>());
            var scopeMock = new Mock<IServiceScope>();
            var scope = scopeMock.Object;
            var context = new RunContext(new RunConfiguration());
            var callbackInvoked = false;
            var endOfPipelineInvoked = false;

            pipeline.InvokeEvent(scope, context, (h, s, c, n) =>
            {
                callbackInvoked = true;

                return default;
            }, (sc, ctxt) =>
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
        public void InvokeEventCanCallHandlersWithNoTarget()
        {
            bool beforeCalled = false;
            bool afterCalled = false;

            var scopeMock = new Mock<IServiceScope>();
            var scope = scopeMock.Object;
            var context = new RunContext(new RunConfiguration());

            var myHandler = new MyEventHandler(() => beforeCalled = true, () => afterCalled = true);

            var pipeline = new EventPipeline(new List<IEventHandler> { myHandler });

            pipeline.InvokeEvent(scope, context, (h, s, c, n) => h.OnExecute(s, c, n));

            beforeCalled.Should().BeTrue();
            afterCalled.Should().BeTrue();
        }

        [Fact]
        public async ValueTask AsyncTargetMethod()
        {
            var order = new List<int>();

            var scopeMock = new Mock<IServiceScope>();
            var scope = scopeMock.Object;
            var context = new RunContext(new RunConfiguration());

            var myHandler = new MyEventHandler(() => order.Add(1), () => order.Add(4));

            var pipeline = new EventPipeline(new List<IEventHandler> { myHandler });

            await pipeline.InvokeEvent(scope, context, (h, s, c, n) => h.OnExecute(s, c, n), async (scope, ctxt) => {
                order.Add(2);
                await Task.Delay(10);
                order.Add(3);
            });

            order.Should().ContainInOrder(new[] { 1, 2, 3, 4 });
        }

        [Fact]
        public async ValueTask EventHandlersInvokedInCorrectOrder()
        {
            var order = new List<int>();

            var scopeMock = new Mock<IServiceScope>();
            var scope = scopeMock.Object;
            var context = new RunContext(new RunConfiguration());

            var myHandler = new MyEventHandler(() => order.Add(1), () => order.Add(8));
            var myHandler2 = new AsyncEventHandler(() => order.Add(2), () => order.Add(7));
            var myHandler3 = new MyEventHandler(() => order.Add(3), () => order.Add(6));

            var pipeline = new EventPipeline(new List<IEventHandler> { myHandler, myHandler2, myHandler3 });

            await pipeline.InvokeEvent(scope, context, (h, s, c, n) => h.OnExecute(s, c, n), async (scope, ctxt) => {
                order.Add(4);
                await Task.Delay(10);
                order.Add(5);
            });

            order.Should().ContainInOrder(new[] { 1, 2, 3, 4, 5, 6, 7, 8 });
        }

        [Fact]
        public async ValueTask StepFailuresThrownAsIs()
        {
            var scopeMock = new Mock<IServiceScope>();
            var scope = scopeMock.Object;
            var context = new RunContext(new RunConfiguration());
            var mockStepReference = new StepReferenceElement();
            Exception? foundException = null;

            var myHandler = new MyEventHandler(ex => foundException = ex);

            var pipeline = new EventPipeline(new List<IEventHandler> { myHandler });

            await pipeline.InvokeEvent(scope, context, (h, s, c, n) => h.OnExecute(s, c, n), (scope, ctxt) => {
                throw new StepFailureException(mockStepReference, new NullReferenceException());
            });

            foundException.Should().NotBeNull();
            foundException.Should().BeOfType<StepFailureException>();
            foundException!.InnerException.Should().BeOfType<NullReferenceException>();
        }

        [Fact]
        public async ValueTask FailuresFromOtherEventHandlerThrownAsIs()
        {
            var scopeMock = new Mock<IServiceScope>();
            var scope = scopeMock.Object;
            var context = new RunContext(new RunConfiguration());
            Exception? foundException = null;

            var myHandler = new MyEventHandler(ex => foundException = ex);
            var errHandler = new MyEventHandler(() => { }, () => throw new EventHandlingException(new NullReferenceException()));

            var pipeline = new EventPipeline(new List<IEventHandler> { myHandler, errHandler });

            await pipeline.InvokeEvent(scope, context, (h, s, c, n) => h.OnExecute(s, c, n));

            foundException.Should().NotBeNull();
            foundException.Should().BeOfType<EventHandlingException>();
            foundException!.InnerException.Should().BeOfType<NullReferenceException>();
        }

        [Fact]
        public async ValueTask RemainingExceptionsWrappedAsEventHandlingException()
        {
            var scopeMock = new Mock<IServiceScope>();
            var scope = scopeMock.Object;
            var context = new RunContext(new RunConfiguration());
            Exception? foundException = null;

            var myHandler = new MyEventHandler(ex => foundException = ex);
            var errHandler = new MyEventHandler(() => { }, () => throw new NullReferenceException());

            var pipeline = new EventPipeline(new List<IEventHandler> { myHandler, errHandler });

            await pipeline.InvokeEvent(scope, context, (h, s, c, n) => h.OnExecute(s, c, n));

            foundException.Should().NotBeNull();
            foundException.Should().BeOfType<EventHandlingException>();
            foundException!.InnerException.Should().BeOfType<NullReferenceException>();
        }

        private class MyEventHandler : IEventHandler
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

            public async ValueTask OnExecute(IServiceScope scope, RunContext ctxt, Func<IServiceScope, RunContext, ValueTask> next)
            {
                callBefore();

                try
                {
                    // We're only going to use execute for testing; the method called has no bearing on the pipeline object.
                    await next(scope, ctxt);
                }
                catch(Exception ex)
                {
                    if(exception != null)
                    {
                        exception(ex);
                    }
                }

                callAfter();
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

            public ValueTask OnThread(IServiceScope scope, ThreadContext ctxt, Func<IServiceScope, ThreadContext, ValueTask> next)
            {
                throw new NotImplementedException();
            }
        }

        private class AsyncEventHandler : IEventHandler
        {
            private readonly Action callBefore;
            private readonly Action callAfter;

            public AsyncEventHandler(Action callBefore, Action callAfter)
            {
                this.callBefore = callBefore;
                this.callAfter = callAfter;
            }

            public async ValueTask OnExecute(IServiceScope scope, RunContext ctxt, Func<IServiceScope, RunContext, ValueTask> next)
            {
                callBefore();

                // We're only going to use execute for testing; the method called has no bearing on the pipeline object.
                await Task.Delay(10);

                await next(scope, ctxt);

                callAfter();
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

            public ValueTask OnThread(IServiceScope scope, ThreadContext ctxt, Func<IServiceScope, ThreadContext, ValueTask> next)
            {
                throw new NotImplementedException();
            }
        }
    }
}
