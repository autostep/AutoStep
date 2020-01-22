using System;

namespace AutoStep.Execution.Dependency
{
    public interface IServiceScope : IDisposable
    {
        string Tag { get; }

        TService Resolve<TService>();

        TServiceType Resolve<TServiceType>(Type serviceType);

        object Resolve(Type serviceType);

        IServiceScope BeginNewScope<TContext>(TContext contextInstance)
            where TContext : TestExecutionContext;

        IServiceScope BeginNewScope<TContext>(string scopeTag, TContext contextInstance)
            where TContext : TestExecutionContext;
    }
}
