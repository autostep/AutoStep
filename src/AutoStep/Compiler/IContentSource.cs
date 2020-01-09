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
    public interface IContentSource
    {
        /// <summary>
        /// Gets the name of the content source, if available.
        /// </summary>
        string? SourceName { get; }

        /// <summary>
        /// Retrieve the last modification time of the file (should be in UTC time).
        /// Sources will not be re-compiled if they have not changed.
        /// </summary>
        /// <returns>The moment at which the source was last modified.</returns>
        DateTime GetLastContentModifyTime();

        /// <summary>
        /// Reads the content of the source.
        /// </summary>
        /// <param name="cancelToken">A cancellation token.</param>
        /// <returns>A task representing the read operation for the content.</returns>
        ValueTask<string> GetContentAsync(CancellationToken cancelToken = default);
    }
}
