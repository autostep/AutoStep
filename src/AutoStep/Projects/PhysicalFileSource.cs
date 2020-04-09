using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AutoStep.Language;

namespace AutoStep.Projects
{
    /// <summary>
    /// Represents a simple file content source that points directly at a file.
    /// </summary>
    public class PhysicalFileSource : IContentSource
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PhysicalFileSource"/> class.
        /// </summary>
        /// <param name="name">The name of the file, typically the relative path from a root folder.</param>
        /// <param name="absolutePath">The absolute path to the file.</param>
        public PhysicalFileSource(string name, string absolutePath)
        {
            SourceName = name;
            AbsolutePath = absolutePath;
        }

        /// <inheritdoc/>
        public string? SourceName { get; }

        /// <summary>
        /// Gets the absolute path to the file.
        /// </summary>
        public string AbsolutePath { get; }

        /// <inheritdoc/>
        public async ValueTask<string> GetContentAsync(CancellationToken cancelToken = default)
        {
            // Allow any IO errors here to throw.
            return await File.ReadAllTextAsync(AbsolutePath, cancelToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public DateTime GetLastContentModifyTime()
        {
            try
            {
                return File.GetLastWriteTime(AbsolutePath);
            }
            catch (IOException)
            {
                // Suggest that it's very old.
                return DateTime.MinValue;
            }
        }
    }
}
