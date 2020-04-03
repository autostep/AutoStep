using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;

namespace AutoStep.Projects.Files
{
    public class FileSet : IFileSet
    {
        private readonly List<FileSetEntry> files = new List<FileSetEntry>();
        private readonly DirectoryInfoBase baseDir;

        public string RootFolder { get; }

        public IReadOnlyList<FileSetEntry> Files => files;

        private FileSet(string rootFolder)
        {
            baseDir = new DirectoryInfoWrapper(new DirectoryInfo(rootFolder));
            RootFolder = rootFolder;
        }

        private FileSet(DirectoryInfoBase directory)
        {
            baseDir = directory;
            RootFolder = directory.FullName;
        }

        internal static FileSet Create(DirectoryInfoBase directory, string[] globs)
        {
            var set = new FileSet(directory);
            set.ScanFiles(globs);
            return set;
        }

        public static FileSet Create(string rootFolder, string[] globs)
        {
            if (!Path.IsPathRooted(rootFolder))
            {
                throw new ArgumentException("File set root folders must be absolute paths.", nameof(rootFolder));
            }

            var set = new FileSet(rootFolder);
            set.ScanFiles(globs);
            return set;
        }

        private void ScanFiles(string[] globs)
        {
            var matcher = new Matcher();
            matcher.AddIncludePatterns(globs);

            var matchResults = matcher.Execute(baseDir);

            foreach (var file in matchResults.Files)
            {
                files.Add(new FileSetEntry { Absolute = Path.GetFullPath(file.Stem, RootFolder), Relative = file.Stem });
            }
        }
    }
}
