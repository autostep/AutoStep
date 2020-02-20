using System;
using System.Collections.Generic;
using System.Linq;
using AutoStep.Execution;
using AutoStep.Execution.Dependency;
using AutoStep.Projects;

namespace AutoStep.Definitions
{
    /// <summary>
    /// Defines a step definition source backed by a project file that has its own step definitions.
    /// </summary>
    internal class FileStepDefinitionSource : IUpdatableStepDefinitionSource
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileStepDefinitionSource"/> class.
        /// </summary>
        /// <param name="file">The file to get steps from.</param>
        public FileStepDefinitionSource(ProjectTestFile file)
        {
            File = file;
        }

        /// <summary>
        /// Gets the unique ID of the source (the file).
        /// </summary>
        public string Uid => File.Path;

        /// <summary>
        /// Gets the name of the file.
        /// </summary>
        public string Name => File.Path;

        /// <summary>
        /// Gets the backing project file.
        /// </summary>
        public ProjectTestFile File { get; }

        /// <summary>
        /// Gets the last modification time of the source, which is equal to the last compilation
        /// time of the file.
        /// </summary>
        /// <returns>The UTC stamp for the file modification time.</returns>
        public DateTime GetLastModifyTime()
        {
            // The last modify time of the source is the last compile output, not the actual file!
            return File.LastCompileTime;
        }

        /// <summary>
        /// Gets the step definitions in the backing file's last compilation.
        /// </summary>
        /// <returns>The set of available definitions.</returns>
        public IEnumerable<StepDefinition> GetStepDefinitions()
        {
            return File.LastCompileResult?.Output?.StepDefinitions.Select(d => new FileStepDefinition(this, d))
                   ?? Enumerable.Empty<FileStepDefinition>();
        }

        /// <inheritdoc/>
        public void ConfigureServices(IServicesBuilder servicesBuilder, RunConfiguration configuration)
        {
            // No additional services needed for these.
        }
    }
}
