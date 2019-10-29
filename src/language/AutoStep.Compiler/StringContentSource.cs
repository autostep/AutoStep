using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace AutoStep.Compiler
{
    public sealed class StringContentSource : IContentSource
    {
        private MemoryStream _stream;

        public StringContentSource(string content)
        {
            _stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        }

        public Stream Open()
        {
            return _stream;
        }

        public void Dispose()
        {
            _stream.Dispose();
        }

        public ValueTask DisposeAsync()
        {
            Dispose();
            return default;
        }
    }
}
