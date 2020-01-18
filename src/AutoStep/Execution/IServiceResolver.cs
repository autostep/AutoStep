using System;

namespace AutoStep.Execution
{
    public interface IServiceResolver
    {
        TService Resolve<TService>();

        TServiceType Resolve<TServiceType>(Type serviceType);
    }
}
