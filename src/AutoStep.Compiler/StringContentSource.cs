using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AutoStep.Compiler
{
    /// <summary>
    /// Represents a content source taken from a constant string.
    /// </summary>
    public sealed class StringContentSource : IContentSource
    {
        private readonly string content;

        /// <summary>
        /// Initializes a new instance of the <see cref="StringContentSource"/> class.
        /// </summary>
        /// <param name="content">The content body.</param>
        public StringContentSource(string content)
        {
            this.content = content;
        }

        /// <inheritdoc/>
        public string? SourceName => null;

        /// <inheritdoc/>
        public ValueTask<string> GetContentAsync(CancellationToken cancelToken = default)
        {
            return new ValueTask<string>(content);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
        }

        /// <inheritdoc/>
        public ValueTask DisposeAsync()
        {
            return default;
        }
    }
}
