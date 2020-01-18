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
        private List<IEventHandler> handlers;
        private readonly ITracer tracer;

        public EventManager(IEnumerable<IEventHandler> allHandlers, ITracer tracer)
        {
            handlers = new List<IEventHandler>(allHandlers);
            this.tracer = tracer;
        }

        public Task InvokeEvent<TContext>(TContext context, Func<IEventHandler, TContext, Func<TContext, Task>, Task> callback, Func<TContext, Task>? next = null)
        {
            if (next is null)
            {
                // This means that there is nothing at the end of the pipeline, create a dummy terminator.
                next = ctxt => default;
            }

            // Need to execute in reverse so we build up the 'next' properly.
            for (int idx = handlers.Count - 1; idx >= 0; idx--)
            {
                next = ChainHandler(next, handlers[idx], callback);
            }

            return next(context);
        }

        private Func<TContext, Task> ChainHandler<TContext>(
            Func<TContext, Task> next,
            IEventHandler innerHandler,
            Func<IEventHandler, TContext, Func<TContext, Task>, Task> callback)
        {
            return ctxt =>
            {
                try
                {
                    return callback(innerHandler, ctxt, next);
                }
                catch (StepFailureException)
                {
                    throw;
                }
                catch (EventHandlingException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    // Anything else is an exception in this handler. Wrap it and throw.
                    throw new EventHandlingException(ex);
                }
            };
        }

        //public async Task<IAsyncDisposable> EnterEventScope<TContext>(TContext context, Func<IEventHandler, TContext, Task> entryEvent, Func<IEventHandler, TContext, Task> exitEvent)
        //{
        //    var triggerResult = new EventTriggerResult();
        //    var triggerScope = new EventTriggerScope<TContext>(this, context, exitEvent);
        //    try
        //    {
        //        // If an exception happens here, automatically run the end scope event as needed.
        //        await TriggerEventAsync(beginHandlerOrder, 0, context, triggerResult, entryEvent);

        //        return triggerScope;
        //    }
        //    catch (Exception ex)
        //    {
        //        await triggerScope.ErrorRecoveryDisposeAsync(ex, triggerResult);

        //        throw new EventHandlingException(ex);
        //    }
        ////}

        //private async Task TriggerEventAsync<TContext>(IList<IEventHandler> handlers, int startHandlerIdx, TContext context, EventTriggerResult? resultHolder, Func<IEventHandler, TContext, Task> callback)
        //{
        //    if (resultHolder is object)
        //    {
        //        resultHolder.SuccessfulHandlerCount = 0;
        //    }

        //    for (var idx = startHandlerIdx; idx < handlers.Count; idx++)
        //    {
        //        var handler = handlers[idx];
        //        try
        //        {
        //            await callback(handler, context).ConfigureAwait(false);

        //            if (resultHolder is object)
        //            {
        //                resultHolder.SuccessfulHandlerCount++;
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            tracer.Error(ex, "Exception occurred in event handler of type {name}", new
        //            {
        //                handler.GetType().Name,
        //            });

        //            throw new EventHandlingException(ex);
        //        }
        //    }
        //}


        //private class EventTriggerScope<TContext> : IAsyncDisposable
        //{
        //    private readonly EventManager manager;
        //    private readonly TContext context;
        //    private readonly Func<IEventHandler, TContext, Task> exitEvent;

        //    public EventTriggerScope(EventManager manager, TContext context, Func<IEventHandler, TContext, Task> exitEvent)
        //    {
        //        this.manager = manager;
        //        this.context = context;
        //        this.exitEvent = exitEvent;
        //    }

        //    public async Task ErrorRecoveryDisposeAsync(Exception cleaningUpAfter, EventTriggerResult openingResult)
        //    {
        //        var cleanupResult = new EventTriggerResult();

        //        try
        //        {
        //            // Start the cleanup at the one that last succeeded.
        //            var startPosition = manager.endHandlerOrder.Count - openingResult.SuccessfulHandlerCount;

        //            // In the disposal of the event scope, we will call the reverse order of
        //            // event handlers (starting at the number we originally ran successfuly.
        //            // This will throw an exception out if a handler failed.
        //            await manager.TriggerEventAsync(manager.endHandlerOrder, startPosition, context, cleanupResult, exitEvent);
        //        }
        //        catch (Exception ex)
        //        {
        //            // The cleanup also generated an error. How frustrating. We'll raise an aggregate exception
        //            // that consists of both the start error and the cleanup error. We don't want to lose the original.
        //            throw new AggregateException(cleaningUpAfter, ex);
        //        }
        //    }

        //    public async Task DisposeAsync()
        //    {
        //        // In the disposal of the event scope, we will call the reverse order of
        //        // event handlers (starting at the number we originally ran successfuly.
        //        // This will throw an exception out if a handler failed.
        //        var cleanupResult = new EventTriggerResult();
        //        await manager.TriggerEventAsync(manager.endHandlerOrder, 0, context, cleanupResult, exitEvent);
        //    }
        //}

        //private class EventTriggerResult
        //{
        //    public int SuccessfulHandlerCount { get; set; }
        //}

    }
}
