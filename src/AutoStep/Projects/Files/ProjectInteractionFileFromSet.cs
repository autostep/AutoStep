using AutoStep.Language;

namespace AutoStep.Projects.Files
{
    /// <summary>
    /// Represents an interaction file that was added from a file set.
    /// </summary>
    internal class ProjectInteractionFileFromSet : ProjectInteractionFile, IProjectFileFromSet
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectInteractionFileFromSet"/> class.
        /// </summary>
        /// <param name="rootPath">The root set path.</param>
        /// <param name="fileEntry">The file entry.</param>
        /// <param name="contentSource">The content source for the file.</param>
        public ProjectInteractionFileFromSet(string rootPath, FileSetEntry fileEntry, IContentSource contentSource)
            : base(fileEntry.Relative, contentSource)
        {
            RootPath = rootPath;
            FileEntry = fileEntry;
        }

        /// <inheritdoc/>
        public FileSetEntry FileEntry { get; }

        /// <inheritdoc/>
        public string RootPath { get; }
    }
}
