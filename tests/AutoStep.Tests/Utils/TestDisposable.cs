using System;

namespace AutoStep.Tests.Utils
{
    public class TestDisposable : IDisposable
    {
        public TestDisposable()
        {
        }

        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            IsDisposed = true;
        }
    }
}
