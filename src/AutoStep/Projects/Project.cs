using System;
using System.Collections.Generic;
using AutoStep.Execution;
using AutoStep.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace AutoStep.Projects
{
    /// <summary>
    /// An AutoStep Project contains one or more test files that can be executed.
    /// </summary>
    public class Project
    {
        private readonly Dictionary<string, ProjectTestFile> allTestFiles = new Dictionary<string, ProjectTestFile>();
        private readonly Dictionary<string, ProjectInteractionFile> allInteractionfiles = new Dictionary<string, ProjectInteractionFile>();

        /// <summary>
        /// Initializes a new instance of the <see cref="Project"/> class.
        /// </summary>
        public Project()
        {
            Compiler = ProjectCompiler.CreateDefault(this);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Project"/> class.
        /// </summary>
        /// <param name="compiler">A custom project compiler.</param>
        public Project(IProjectCompiler compiler)
        {
            Compiler = compiler ?? throw new ArgumentNullException(nameof(compiler));
        }

        /// <summary>
        /// Gets the set of all files in the project.
        /// </summary>
        public IReadOnlyDictionary<string, ProjectTestFile> AllTestFiles => allTestFiles;

        public IReadOnlyDictionary<string, ProjectInteractionFile> AllInteractionFiles => allInteractionfiles;

        /// <summary>
        /// Gets the active project configuration.
        /// </summary>
        public ProjectConfiguration? Configuration { get; }

        /// <summary>
        /// Gets the project compiler.
        /// </summary>
        public IProjectCompiler Compiler { get; }

        /// <summary>
        /// Attempts to add a file to the project (will return false if it's already in the project).
        /// </summary>
        /// <param name="file">The file to add.</param>
        /// <returns>True if the file was added.</returns>
        public bool TryAddFile(ProjectTestFile file)
        {
            if (file is null)
            {
                throw new ArgumentNullException(nameof(file));
            }

            if (allTestFiles.TryAdd(file.Path, file))
            {
                file.IsAttachedToProject = true;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Attempts to add an interaction file to the project (will return false if it's already in the project).
        /// </summary>
        /// <param name="file">The file to add.</param>
        /// <returns>True if the file was added.</returns>
        public bool TryAddFile(ProjectInteractionFile file)
        {
            if (file is null)
            {
                throw new ArgumentNullException(nameof(file));
            }

            if (allInteractionfiles.TryAdd(file.Path, file))
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
        public bool TryRemoveFile(ProjectTestFile file)
        {
            if (file is null)
            {
                throw new ArgumentNullException(nameof(file));
            }

            if (allTestFiles.Remove(file.Path))
            {
                file.IsAttachedToProject = false;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Create a test run with defaults.
        /// </summary>
        /// <returns>The test run.</returns>
        public TestRun CreateTestRun()
        {
            return new TestRun(this, new RunConfiguration());
        }

        /// <summary>
        /// Create a test run with the specified configuration.
        /// </summary>
        /// <param name="configuration">The run configuration.</param>
        /// <returns>The test run.</returns>
        public TestRun CreateTestRun(RunConfiguration configuration)
        {
            return new TestRun(this, configuration);
        }

        /// <summary>
        /// Create a test run with the specified configuration and feature filter.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="filter">The feature filter.</param>
        /// <returns>The test run.</returns>
        public TestRun CreateTestRun(RunConfiguration configuration, IRunFilter filter)
        {
            return new TestRun(this, configuration, filter);
        }
    }
}
