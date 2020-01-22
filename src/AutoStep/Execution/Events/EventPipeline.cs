using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoStep.Execution.Dependency;

namespace AutoStep.Execution.Events
{
    internal class EventPipeline : IEventPipeline
    {
        private IReadOnlyList<IEventHandler> handlers;

        public EventPipeline(IReadOnlyList<IEventHandler> handlers)
        {
            this.handlers = handlers;
        }

        public void ConfigureServices(IServicesBuilder builder, RunConfiguration configuration)
        {
            foreach (var handler in handlers)
            {
                handler.ConfigureServices(builder, configuration);
            }
        }

        public Task InvokeEvent<TContext>(
            IServiceScope scope,
            TContext context,
            Func<IEventHandler, IServiceScope, TContext, Func<IServiceScope, TContext, Task>, Task> callback,
            Func<IServiceScope, TContext, Task>? next = null)
        {
            if (next is null)
            {
                // This means that there is nothing at the end of the pipeline, create a dummy terminator.
                next = (s, c) => Task.CompletedTask;
            }

            // Need to execute in reverse so we build up the 'next' properly.
            for (var idx = handlers.Count - 1; idx >= 0; idx--)
            {
                next = ChainHandler(next, handlers[idx], callback);
            }

            return next(scope, context);
        }

        private Func<IServiceScope, TContext, Task> ChainHandler<TContext>(
            Func<IServiceScope, TContext, Task> next,
            IEventHandler innerHandler,
            Func<IEventHandler, IServiceScope, TContext, Func<IServiceScope, TContext, Task>, Task> callback)
        {
            return async (resolver, ctxt) =>
            {
                try
                {
                    await callback(innerHandler, resolver, ctxt, next).ConfigureAwait(false);
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
    }
}
