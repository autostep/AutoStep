using System;
using System.Threading.Tasks;
using AutoStep.Execution.Dependency;

namespace AutoStep.Execution.Events
{
    public interface IEventPipeline
    {
        ValueTask InvokeEvent<TContext>(
                    IServiceScope scope,
                    TContext context,
                    Func<IEventHandler, IServiceScope, TContext, Func<IServiceScope, TContext, ValueTask>, ValueTask> callback,
                    Func<IServiceScope, TContext, ValueTask>? next = null);
    }
}
