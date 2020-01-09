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

        public IReadOnlyDictionary<string, ProjectFile> AllFiles => allFiles;

        public ProjectConfiguration Configuration { get; }

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

    /// <summary>
    /// Represents the configuration for the project.
    /// This + environment variables = run configuration.
    /// </summary>
    public class ProjectConfiguration
    {

    }
}
