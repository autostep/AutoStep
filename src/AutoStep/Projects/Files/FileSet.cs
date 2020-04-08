using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;

namespace AutoStep.Projects.Files
{
    public class FileSet : IFileSet
    {
        private readonly List<FileSetEntry> files = new List<FileSetEntry>();
        private readonly DirectoryInfoBase baseDir;
        private readonly Matcher matcher;

        public string RootFolder { get; }

        public IReadOnlyList<FileSetEntry> Files => files;

        internal static FileSet Create(DirectoryInfoBase directory, string[] includeGlobs, string[]? excludeGlobs = null)
        {
            var set = new FileSet(directory, includeGlobs, excludeGlobs);
            set.ScanFiles();
            return set;
        }

        public static FileSet Create(string rootFolder, string[] includeGlobs, string[]? excludeGlobs = null)
        {
            if (!Path.IsPathRooted(rootFolder))
            {
                throw new ArgumentException("File set root folders must be absolute paths.", nameof(rootFolder));
            }

            var set = new FileSet(rootFolder, includeGlobs, excludeGlobs);

            set.ScanFiles();

            return set;
        }

        public bool TryAddFile(string relativePath)
        {
            var match = matcher.Match(relativePath);

            if (match.HasMatches)
            {
                var file = match.Files.First();

                if (files.All(f => f.Relative != relativePath))
                {
                    files.Add(new FileSetEntry { Absolute = Path.GetFullPath(file.Path, RootFolder), Relative = file.Path });

                    return true;
                }
            }

            return false;
        }

        public bool TryRemoveFile(string relativePath)
        {
            var indexOf = files.FindIndex(f => f.Relative == relativePath);

            if (indexOf > -1)
            {
                files.RemoveAt(indexOf);
            }

            return false;
        }

        private FileSet(string rootFolder, string[] includeGlobs, string[]? excludeGlobs = null)
        {
            baseDir = new DirectoryInfoWrapper(new DirectoryInfo(rootFolder));
            matcher = GetMatcher(includeGlobs, excludeGlobs);
            RootFolder = rootFolder;
        }

        private FileSet(DirectoryInfoBase directory, string[] includeGlobs, string[]? excludeGlobs = null)
        {
            baseDir = directory;
            matcher = GetMatcher(includeGlobs, excludeGlobs);
            RootFolder = directory.FullName;
        }

        private Matcher GetMatcher(string[] includeGlobs, string[]? excludeGlobs)
        {
            var matcher = new Matcher();
            matcher.AddIncludePatterns(includeGlobs);

            if (excludeGlobs is object)
            {
                matcher.AddExcludePatterns(excludeGlobs);
            }

            return matcher;
        }

        private void ScanFiles()
        {
            var matchResults = matcher.Execute(baseDir);

            foreach (var file in matchResults.Files)
            {
                files.Add(new FileSetEntry { Absolute = Path.GetFullPath(file.Path, RootFolder), Relative = file.Path });
            }
        }
    }
}
