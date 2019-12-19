using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AutoStep.Compiler
{
    /// <summary>
    /// Defines an interface for anything that provides content for the autostep compiler.
    /// </summary>
    public interface IContentSource : IDisposable, IAsyncDisposable
    {
        /// <summary>
        /// Gets the name of the content source, if available.
        /// </summary>
        string? SourceName { get; }

        /// <summary>
        /// Reads the content of the source.
        /// </summary>
        /// <param name="cancelToken">A cancellation token.</param>
        /// <returns>A task representing the read operation for the content.</returns>
        ValueTask<string> GetContentAsync(CancellationToken cancelToken = default);
    }
}
