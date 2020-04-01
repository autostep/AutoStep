using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AutoStep.Language;

namespace AutoStep.Projects.Configuration
{
    public class PhysicalFileSource : IContentSource
    {
        public PhysicalFileSource(string name, string absolutePath)
        {
            SourceName = name;
            AbsolutePath = absolutePath;
        }

        public string? SourceName { get; }

        public string AbsolutePath { get; }

        public async ValueTask<string> GetContentAsync(CancellationToken cancelToken = default)
        {
            // Allow any IO errors here to throw.
            return await File.ReadAllTextAsync(AbsolutePath, cancelToken);
        }

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
