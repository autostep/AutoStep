using System;
using System.IO;
using System.Text;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

namespace AutoStep.Tests.Utils
{
    public class DummyFileProvider : IFileProvider
    {
        private readonly DummyFileInfo fileInfo;

        public DummyFileProvider()
        {
        }

        public DummyFileProvider(string fileContent, DateTime fileLastModifyTime)
        {
            fileInfo = new DummyFileInfo(fileContent, fileLastModifyTime);
        }

        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            throw new NotImplementedException();
        }

        public IFileInfo GetFileInfo(string subpath)
        {
            if (fileInfo is null)
            {
                throw new InvalidOperationException();
            }

            return fileInfo;
        }

        public IChangeToken Watch(string filter)
        {
            throw new NotImplementedException();
        }

        private class DummyFileInfo : IFileInfo
        {
            private readonly string fileContent;
            private readonly DateTime lastModified;

            public DummyFileInfo(string fileContent, DateTime lastModified)
            {
                this.fileContent = fileContent;
                this.lastModified = lastModified;
            }

            public bool Exists => throw new NotImplementedException();

            public long Length => throw new NotImplementedException();

            public string PhysicalPath => throw new NotImplementedException();

            public string Name => throw new NotImplementedException();

            public DateTimeOffset LastModified => lastModified;

            public bool IsDirectory => false;

            public Stream CreateReadStream()
            {
                var stream = new MemoryStream();

                stream.Write(Encoding.UTF8.GetBytes(fileContent));

                stream.Seek(0, SeekOrigin.Begin);

                return stream;
            }
        }
    }
}
