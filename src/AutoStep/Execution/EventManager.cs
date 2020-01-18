using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoStep.Tracing;

namespace AutoStep.Execution
{
    /// <summary>
    /// Make this a pipeline!!!
    /// </summary>
    internal class EventManager
    {
        private List<IEventHandler> beginHandlerOrder;
        private List<IEventHandler> endHandlerOrder;
        private readonly ITracer tracer;

        public EventManager(IEnumerable<IEventHandler> allHandlers, ITracer tracer)
        {
            beginHandlerOrder = new List<IEventHandler>(allHandlers);
            endHandlerOrder = new List<IEventHandler>(allHandlers.Reverse());
            this.tracer = tracer;
        }

        public ValueTask InvokeEvent<TContext>(TContext context, Func<IEventHandler, TContext, ValueTask> callback)
        {
            return TriggerEventAsync(beginHandlerOrder, 0, context, null, callback);
        }

        public async ValueTask<IAsyncDisposable> EnterEventScope<TContext>(TContext context, Func<IEventHandler, TContext, ValueTask> entryEvent, Func<IEventHandler, TContext, ValueTask> exitEvent)
        {
            var triggerResult = new EventTriggerResult();
            var triggerScope = new EventTriggerScope<TContext>(this, context, exitEvent);
            try
            {
                // If an exception happens here, automatically run the end scope event as needed.
                await TriggerEventAsync(beginHandlerOrder, 0, context, triggerResult, entryEvent);

                return triggerScope;
            }
            catch (Exception ex)
            {
                await triggerScope.ErrorRecoveryDisposeAsync(ex, triggerResult);

                throw new EventHandlingException(ex);
            }
        }

        private async ValueTask TriggerEventAsync<TContext>(IList<IEventHandler> handlers, int startHandlerIdx, TContext context, EventTriggerResult? resultHolder, Func<IEventHandler, TContext, ValueTask> callback)
        {
            if (resultHolder is object)
            {
                resultHolder.SuccessfulHandlerCount = 0;
            }

            for (var idx = startHandlerIdx; idx < handlers.Count; idx++)
            {
                var handler = handlers[idx];
                try
                {
                    await callback(handler, context).ConfigureAwait(false);

                    if (resultHolder is object)
                    {
                        resultHolder.SuccessfulHandlerCount++;
                    }
                }
                catch (Exception ex)
                {
                    tracer.Error(ex, "Exception occurred in event handler of type {name}", new
                    {
                        handler.GetType().Name,
                    });

                    throw new EventHandlingException(ex);
                }
            }
        }


        private class EventTriggerScope<TContext> : IAsyncDisposable
        {
            private readonly EventManager manager;
            private readonly TContext context;
            private readonly Func<IEventHandler, TContext, ValueTask> exitEvent;

            public EventTriggerScope(EventManager manager, TContext context, Func<IEventHandler, TContext, ValueTask> exitEvent)
            {
                this.manager = manager;
                this.context = context;
                this.exitEvent = exitEvent;
            }

            public async ValueTask ErrorRecoveryDisposeAsync(Exception cleaningUpAfter, EventTriggerResult openingResult)
            {
                var cleanupResult = new EventTriggerResult();

                try
                {
                    // Start the cleanup at the one that last succeeded.
                    var startPosition = manager.endHandlerOrder.Count - openingResult.SuccessfulHandlerCount;

                    // In the disposal of the event scope, we will call the reverse order of
                    // event handlers (starting at the number we originally ran successfuly.
                    // This will throw an exception out if a handler failed.
                    await manager.TriggerEventAsync(manager.endHandlerOrder, startPosition, context, cleanupResult, exitEvent);
                }
                catch (Exception ex)
                {
                    // The cleanup also generated an error. How frustrating. We'll raise an aggregate exception
                    // that consists of both the start error and the cleanup error. We don't want to lose the original.
                    throw new AggregateException(cleaningUpAfter, ex);
                }
            }

            public async ValueTask DisposeAsync()
            {
                // In the disposal of the event scope, we will call the reverse order of
                // event handlers (starting at the number we originally ran successfuly.
                // This will throw an exception out if a handler failed.
                var cleanupResult = new EventTriggerResult();
                await manager.TriggerEventAsync(manager.endHandlerOrder, 0, context, cleanupResult, exitEvent);
            }
        }

        private class EventTriggerResult
        {
            public int SuccessfulHandlerCount { get; set; }
        }

    }
}
