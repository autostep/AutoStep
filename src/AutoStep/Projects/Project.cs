using System;
using System.Collections.Generic;
using AutoStep.Execution;
using Microsoft.Extensions.Configuration;

namespace AutoStep.Projects
{
    /// <summary>
    /// An AutoStep Project contains one or more test files that can be executed.
    /// </summary>
    public class Project
    {
        private readonly Dictionary<string, ProjectFile> allFiles = new Dictionary<string, ProjectFile>();

        private int lastInteractionFileOrder = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="Project"/> class.
        /// </summary>
        /// <param name="forEditing">
        /// If set to true, then the compiler behaviour will be amended to support editing functionality.
        /// For example, generating intellisense indexing.
        /// </param>
        public Project(bool forEditing = false)
        {
            if (forEditing)
            {
                Builder = ProjectBuilder.CreateForEditing(this);
            }
            else
            {
                Builder = ProjectBuilder.CreateDefault(this);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Project"/> class, using a custom
        /// project builder factory.
        /// </summary>
        /// <param name="builderFactory">
        /// Factory to create a builder instance.
        /// </param>
        public Project(Func<Project, IProjectBuilder> builderFactory)
        {
            if (builderFactory is null)
            {
                throw new ArgumentNullException(nameof(builderFactory));
            }

            var createdBuilder = builderFactory(this);

            if (createdBuilder is null)
            {
                throw new InvalidOperationException(ProjectMessages.ProjectBuilderCannotReturnNull);
            }

            Builder = createdBuilder;
        }

        /// <summary>
        /// Gets the set of all files in the project.
        /// </summary>
        public IReadOnlyDictionary<string, ProjectFile> AllFiles => allFiles;

        /// <summary>
        /// Gets the project builder.
        /// </summary>
        public IProjectBuilder Builder { get; }

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

            if (allFiles.TryAdd(file.Path, file))
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

            if (allFiles.TryAdd(file.Path, file))
            {
                lastInteractionFileOrder++;
                file.Order = lastInteractionFileOrder;
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

        /// <summary>
        /// Create a test run with defaults.
        /// </summary>
        /// <returns>The test run.</returns>
        public TestRun CreateTestRun()
        {
            // No configuration to speak of.
            return new TestRun(this, new ConfigurationBuilder().Build());
        }

        /// <summary>
        /// Create a test run with the specified configuration.
        /// </summary>
        /// <param name="projectConfiguration">The project configuration.</param>
        /// <returns>The test run.</returns>
        public TestRun CreateTestRun(IConfiguration projectConfiguration)
        {
            return new TestRun(this, projectConfiguration);
        }

        /// <summary>
        /// Create a test run with the specified configuration and feature filter.
        /// </summary>
        /// <param name="projectConfiguration">The configuration.</param>
        /// <param name="filter">The feature filter.</param>
        /// <returns>The test run.</returns>
        public TestRun CreateTestRun(IConfiguration projectConfiguration, IRunFilter filter)
        {
            return new TestRun(this, projectConfiguration, filter);
        }
    }
}
