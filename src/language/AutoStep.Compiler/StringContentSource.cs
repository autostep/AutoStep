using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AutoStep.Compiler
{
    public sealed class StringContentSource : IContentSource
    {
        private string content;

        public string SourceName => null;

        public StringContentSource(string content)
        {
            this.content = content;
        }

        public ValueTask<string> GetContentAsync(CancellationToken cancelToken = default)
        {
            return new ValueTask<string>(content);
        }

        public void Dispose()
        {
        }

        public ValueTask DisposeAsync()
        {
            return default;
        }
    }
}
