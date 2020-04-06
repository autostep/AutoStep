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

        internal static FileSet Create(DirectoryInfoBase directory, string[] includeGlobs, string[]? excludeGlobs = null)
        {
            var set = new FileSet(directory);
            set.ScanFiles(includeGlobs, excludeGlobs);
            return set;
        }

        public static FileSet Create(string rootFolder, string[] includeGlobs, string[]? excludeGlobs = null)
        {
            if (!Path.IsPathRooted(rootFolder))
            {
                throw new ArgumentException("File set root folders must be absolute paths.", nameof(rootFolder));
            }

            var set = new FileSet(rootFolder);
            set.ScanFiles(includeGlobs, excludeGlobs);
            return set;
        }

        private void ScanFiles(string[] includeGlobs, string[]? excludeGlobs)
        {
            var matcher = new Matcher();
            matcher.AddIncludePatterns(includeGlobs);

            if (excludeGlobs is object)
            {
                matcher.AddExcludePatterns(excludeGlobs);
            }

            var matchResults = matcher.Execute(baseDir);

            foreach (var file in matchResults.Files)
            {
                files.Add(new FileSetEntry { Absolute = Path.GetFullPath(file.Stem, RootFolder), Relative = file.Stem });
            }
        }
    }
}
