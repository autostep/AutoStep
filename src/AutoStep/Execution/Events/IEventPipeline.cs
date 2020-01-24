using System;
using System.Threading.Tasks;
using AutoStep.Execution.Dependency;

namespace AutoStep.Execution.Events
{
    /// <summary>
    /// Defines an interface to a pipeline for executing events.
    /// </summary>
    public interface IEventPipeline
    {
        /// <summary>
        /// Invoke an event, optionally with a final callback to invoke at the end of the pipeline.
        /// </summary>
        /// <typeparam name="TContext">The context type.</typeparam>
        /// <param name="scope">The current scope.</param>
        /// <param name="context">The relevant context object.</param>
        /// <param name="callback">The callback to invoke on each <see cref="IEventHandler"/> object.</param>
        /// <param name="final">An optional callback to invoke at the end of the pipeline.</param>
        /// <returns>A completion task.</returns>
        ValueTask InvokeEvent<TContext>(
                    IServiceScope scope,
                    TContext context,
                    Func<IEventHandler, IServiceScope, TContext, Func<IServiceScope, TContext, ValueTask>, ValueTask> callback,
                    Func<IServiceScope, TContext, ValueTask>? final = null);
    }
}
