using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AutoStep.Compiler;
using Microsoft.Extensions.FileProviders;

namespace AutoStep
{
    public class ProjectFileContentSource : IContentSource
    {
        private readonly IFileProvider fileProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectFileContentSource"/> class.
        /// </summary>
        /// <param name="fullPath">Full path to the file.</param>
        /// <param name="fileProvider">Provider of IO.</param>
        public ProjectFileContentSource(string fullPath, IFileProvider fileProvider)
        {
            this.SourceName = fullPath;
            this.fileProvider = fileProvider ?? throw new ArgumentNullException(nameof(fileProvider));
        }

        public DateTime GetLastContentModifyTime()
        {
            return fileProvider.GetFileInfo(SourceName).LastModified.UtcDateTime;
        }

        public string? SourceName { get; }

        public async ValueTask<string> GetContentAsync(CancellationToken cancelToken = default)
        {
            // Create a stream reader that doesn't close the file on dispose, so we can do an async dispose.
            using (var stream = fileProvider.GetFileInfo(SourceName).CreateReadStream())
            using (var streamReader = new StreamReader(stream))
            {
                return await streamReader.ReadToEndAsync().ConfigureAwait(false);
            }
        }
    }
}
