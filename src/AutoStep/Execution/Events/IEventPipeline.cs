using System;
using System.Threading.Tasks;
using AutoStep.Execution.Dependency;

namespace AutoStep.Execution.Events
{
    public interface IEventPipeline
    {
        Task InvokeEvent<TContext>(
            IServiceScope scope,
            TContext context,
            Func<IEventHandler, IServiceScope, TContext, Func<IServiceScope, TContext, Task>, Task> callback,
            Func<IServiceScope, TContext, Task>? next = null);
    }
}
