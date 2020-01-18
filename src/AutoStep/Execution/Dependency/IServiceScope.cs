using System;

namespace AutoStep.Execution.Dependency
{
    public interface IServiceScope : IServiceResolver, IDisposable
    {
        IServiceScope BeginNewScope(string scopeTag);
    }
}
