using System;

namespace AutoStep.Execution
{
    public interface IServiceScope : IServiceResolver, IDisposable
    {
        IServiceScope BeginNewScope(string scopeTag);
    }
}
