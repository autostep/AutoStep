using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AutoStep.Compiler;

namespace AutoStep
{
    /// <summary>
    /// An AutoStep Project contains one or more test files that can be executed.
    /// </summary>
    public class Project
    {
        private Dictionary<string, ProjectFile> allFiles = new Dictionary<string, ProjectFile>();

        /// <summary>
        /// Gets the set of all files in the project.
        /// </summary>
        public IReadOnlyDictionary<string, ProjectFile> AllFiles => allFiles;

        /// <summary>
        /// Gets the active project configuration.
        /// </summary>
        public ProjectConfiguration? Configuration { get; }

        /// <summary>
        /// Attempts to add a file to the project (will return false if it's already in the project).
        /// </summary>
        /// <param name="file">The file to add.</param>
        /// <returns>True if the file was added.</returns>
        public bool TryAddFile(ProjectFile file)
        {
            if (file is null)
            {
                throw new ArgumentNullException(nameof(file));
            }

            if (allFiles.TryAdd(file.Path, file))
            {
                file.IsAttachedToProject = true;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Attempts to remove a file from the project (will return false if it's not in the project).
        /// </summary>
        /// <param name="file">The file to remove.</param>
        /// <returns>True if the file was removed.</returns>
        public bool TryRemoveFile(ProjectFile file)
        {
            if (file is null)
            {
                throw new ArgumentNullException(nameof(file));
            }

            if (allFiles.Remove(file.Path))
            {
                file.IsAttachedToProject = false;
                return true;
            }

            return false;
        }
    }
}
