using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AutoStep.Compiler
{
    public sealed class StringContentSource : IContentSource
    {
        private StringReader reader;

        public string? SourceName => null;

        /// <summary>
        /// UTF-16 for .net strings.
        /// </summary>
        public Encoding Encoding => Encoding.Unicode;

        public StringContentSource(string content)
        {
            reader = new StringReader(content);
        }

        public ValueTask<TextReader> GetReaderAsync(CancellationToken cancelToken = default)
        {
            return new ValueTask<TextReader>(reader);
        }

        public void Dispose()
        {
            reader.Dispose();
        }

        public ValueTask DisposeAsync()
        {
            Dispose();
            return default;
        }
    }
}
