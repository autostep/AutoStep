using System;
using System.IO;

namespace AutoStep.Compiler
{
    public interface IContentSource : IDisposable, IAsyncDisposable
    {
        Stream Open();
    }
}
