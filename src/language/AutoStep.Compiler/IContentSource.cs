using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AutoStep.Compiler
{
    public interface IContentSource : IDisposable, IAsyncDisposable
    {
        ValueTask<string> GetContentAsync(CancellationToken cancelToken = default);

        string? SourceName { get; }
    }
}
