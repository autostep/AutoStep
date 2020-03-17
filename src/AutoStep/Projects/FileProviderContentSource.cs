using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AutoStep.Language;
using Microsoft.Extensions.FileProviders;

namespace AutoStep.Projects
{
    /// <summary>
    /// Defines a content source that loads a project file from an <see cref="IFileProvider" />.
    /// </summary>
    public class FileProviderContentSource : IContentSource
    {
        private readonly IFileProvider fileProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileProviderContentSource"/> class.
        /// </summary>
        /// <param name="path">Path to the file.</param>
        /// <param name="fileProvider">Provider of IO.</param>
        public FileProviderContentSource(string path, IFileProvider fileProvider)
        {
            SourceName = path;
            this.fileProvider = fileProvider ?? throw new ArgumentNullException(nameof(fileProvider));
        }

        /// <summary>
        /// Gets the source name of the file (the path).
        /// </summary>
        public string? SourceName { get; }

        /// <summary>
        /// Gets the time the file was last changed.
        /// </summary>
        /// <returns>The time in UTC of file modification.</returns>
        public DateTime GetLastContentModifyTime()
        {
            return fileProvider.GetFileInfo(SourceName).LastModified.UtcDateTime;
        }

        /// <summary>
        /// Reads the content of the file.
        /// </summary>
        /// <param name="cancelToken">Cancellation token (has no effect on this source).</param>
        /// <returns>The content of the source.</returns>
        public async ValueTask<string> GetContentAsync(CancellationToken cancelToken = default)
        {
            await using var stream = fileProvider.GetFileInfo(SourceName).CreateReadStream();
            using var streamReader = new StreamReader(stream);
            return await streamReader.ReadToEndAsync().ConfigureAwait(false);
        }
    }
}
