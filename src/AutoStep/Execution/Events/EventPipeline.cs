using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoStep.Execution.Dependency;

namespace AutoStep.Execution.Events
{
    /// <summary>
    /// Provides the functionality to manage a pipeline of event handlers.
    /// </summary>
    internal class EventPipeline : IEventPipeline
    {
        private IReadOnlyList<IEventHandler> handlers;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventPipeline"/> class.
        /// </summary>
        /// <param name="handlers">The set of handlers.</param>
        public EventPipeline(IReadOnlyList<IEventHandler> handlers)
        {
            this.handlers = handlers;
        }

        /// <inheritdoc/>
        public ValueTask InvokeEvent<TContext>(
            IServiceProvider serviceProvider,
            TContext context,
            Func<IEventHandler, IServiceProvider, TContext, Func<IServiceProvider, TContext, ValueTask>, ValueTask> callback,
            Func<IServiceProvider, TContext, ValueTask>? final = null)
        {
            if (final is null)
            {
                // This means that there is nothing at the end of the pipeline, create a dummy terminator.
                final = (s, c) => default;
            }

            // Need to execute in reverse so we build up the 'next' properly.
            for (var idx = handlers.Count - 1; idx >= 0; idx--)
            {
                final = ChainHandler(final, handlers[idx], callback);
            }

            return final(serviceProvider, context);
        }

        private Func<IServiceProvider, TContext, ValueTask> ChainHandler<TContext>(
            Func<IServiceProvider, TContext, ValueTask> next,
            IEventHandler innerHandler,
            Func<IEventHandler, IServiceProvider, TContext, Func<IServiceProvider, TContext, ValueTask>, ValueTask> callback)
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
