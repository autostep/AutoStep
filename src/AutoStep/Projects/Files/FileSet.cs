using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;

namespace AutoStep.Projects.Files
{
    /// <summary>
    /// Represents a globbed set of files found in a specific root directory.
    /// </summary>
    /// <remarks>
    /// Backed by the Microsoft.Extensions.FileSystemGlobbing library.
    /// </remarks>
    public class FileSet : IFileSet
    {
        private readonly List<FileSetEntry> files = new List<FileSetEntry>();
        private readonly DirectoryInfoBase baseDir;
        private readonly Matcher matcher;

        /// <inheritdoc/>
        public string RootFolder { get; }

        /// <inheritdoc/>
        public IReadOnlyList<FileSetEntry> Files => files;

        /// <summary>
        /// Create a new file set.
        /// </summary>
        /// <param name="rootFolder">The root folder.</param>
        /// <param name="includeGlobs">The set of globs to include in the set.</param>
        /// <param name="excludeGlobs">The (optional) set of globs to exclude from the set.</param>
        /// <returns>The new file set.</returns>
        public static FileSet Create(string rootFolder, string[] includeGlobs, string[]? excludeGlobs = null)
        {
            if (!Path.IsPathRooted(rootFolder))
            {
                throw new ArgumentException(FileSetMessages.RootFoldersMustBeAbsolute, nameof(rootFolder));
            }

            var set = new FileSet(rootFolder, includeGlobs, excludeGlobs);

            set.ScanFiles();

            return set;
        }

        /// <inheritdoc/>
        public bool TryAddFile(string relativePath)
        {
            var match = matcher.Match(relativePath);

            if (match.HasMatches)
            {
                var file = match.Files.First();

                if (files.All(f => f.Relative != relativePath))
                {
                    files.Add(new FileSetEntry(Path.GetFullPath(file.Path, RootFolder), file.Path));

                    return true;
                }
            }

            return false;
        }

        /// <inheritdoc/>
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
                files.Add(new FileSetEntry(Path.GetFullPath(file.Path, RootFolder), file.Path));
            }
        }
    }
}
