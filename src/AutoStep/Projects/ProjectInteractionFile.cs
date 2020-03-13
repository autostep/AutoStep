using System;
using AutoStep.Language;
using AutoStep.Language.Interaction;

namespace AutoStep.Projects
{
    /// <summary>
    /// Represents a single file within a project.
    /// </summary>
    public class ProjectInteractionFile : ProjectFile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectInteractionFile"/> class.
        /// </summary>
        /// <param name="filePath">The path to the file (might be physical or not, this is the identifier for the file).</param>
        /// <param name="contentSource">The content source used to access the raw file content.</param>
        public ProjectInteractionFile(string filePath, IContentSource contentSource)
            : base(filePath, contentSource)
        {
            LastCompileTime = DateTime.MinValue;
            LastSetBuildTime = DateTime.MinValue;
        }

        /// <summary>
        /// Gets the result of the last compilation operation on this project file.
        /// </summary>
        public InteractionsFileCompilerResult? LastCompileResult { get; private set; }

        /// <summary>
        /// Gets the timestamp (UTC) of the last compilation result (successful or otherwise).
        /// </summary>
        public DateTime LastCompileTime { get; private set; }

        /// <summary>
        /// Gets the result of the last set build operation on this interaction file.
        /// </summary>
        public InteractionsFileSetBuildResult? LastSetBuildResult { get; private set; }

        /// <summary>
        /// Gets the timestamp (UTC) of the last set build (successful or otherwise).
        /// </summary>
        public DateTime LastSetBuildTime { get; private set; }

        /// <summary>
        /// Gets or sets the order in which interaction files are considered. This can be important for component inheritance.
        /// Higher values mean that a file is considered later (more specific).
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Update this file with the latest compilation result.
        /// </summary>
        /// <param name="result">The result to update from.</param>
        public void UpdateLastCompileResult(InteractionsFileCompilerResult result)
        {
            LastCompileResult = result ?? throw new ArgumentNullException(nameof(result));
            LastCompileTime = DateTime.UtcNow;
        }

        /// <summary>
        /// Update this file with the latest set build result.
        /// </summary>
        /// <param name="result">The result to update from.</param>
        public void UpdateLastSetBuildResult(InteractionsFileSetBuildResult result)
        {
            LastSetBuildResult = result ?? throw new ArgumentNullException(nameof(result));
            LastSetBuildTime = DateTime.UtcNow;
        }
    }
}
