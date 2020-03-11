using System;
using AutoStep.Language;
using AutoStep.Language.Interaction;

namespace AutoStep.Projects
{
    /// <summary>
    /// Represents a single file within a project.
    /// </summary>
    public class ProjectInteractionFile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectInteractionFile"/> class.
        /// </summary>
        /// <param name="filePath">The path to the file (might be physical or not, this is the identifier for the file).</param>
        /// <param name="contentSource">The content source used to access the raw file content.</param>
        public ProjectInteractionFile(string filePath, IContentSource contentSource)
        {
            if (filePath is null)
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            Path = filePath;
            ContentSource = contentSource ?? throw new ArgumentNullException(nameof(contentSource));
            LastCompileTime = DateTime.MinValue;
        }

        /// <summary>
        /// Gets the path to the file (note that this might not be a physical file path, it could be a URL or similar).
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// Gets the <see cref="IContentSource"/> used to access the underlying file content.
        /// </summary>
        public IContentSource ContentSource { get; }

        /// <summary>
        /// Gets the result of the last compilation operation on this project file.
        /// </summary>
        public InteractionsFileCompilerResult? LastCompileResult { get; private set; }

        /// <summary>
        /// Gets the timestamp (UTC) of the last compilation result (successful or otherwise).
        /// </summary>
        public DateTime LastCompileTime { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether this file is attached to a project. A detached file will not be compiled or linked as part of a project.
        /// </summary>
        public bool IsAttachedToProject { get; set; }

        /// <summary>
        /// Update this file with the latest compilation result.
        /// </summary>
        /// <param name="result">The result to update from.</param>
        public void UpdateLastCompileResult(InteractionsFileCompilerResult result)
        {
            LastCompileResult = result ?? throw new ArgumentNullException(nameof(result));
            LastCompileTime = DateTime.UtcNow;
        }
    }
}
