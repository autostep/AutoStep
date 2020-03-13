using System;
using AutoStep.Language;

namespace AutoStep.Projects
{
    /// <summary>
    /// Base class for project files.
    /// </summary>
    public abstract class ProjectFile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectFile"/> class.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="contentSource">The content source.</param>
        public ProjectFile(string filePath, IContentSource contentSource)
        {
            if (filePath is null)
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            Path = filePath;
            ContentSource = contentSource ?? throw new ArgumentNullException(nameof(contentSource));
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
        /// Gets or sets a value indicating whether this file is attached to a project. A detached file will not be compiled or linked as part of a project.
        /// </summary>
        public bool IsAttachedToProject { get; set; }
    }
}
