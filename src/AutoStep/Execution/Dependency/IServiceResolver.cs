using System;

namespace AutoStep.Execution.Dependency
{
    public interface IServiceResolver
    {
        TService Resolve<TService>();

        TServiceType Resolve<TServiceType>(Type serviceType);
    }
}
