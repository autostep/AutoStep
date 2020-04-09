namespace AutoStep.Projects.Files
{
    /// <summary>
    /// Indicates that a project file was built from a set, so has root path and file entry details.
    /// </summary>
    public interface IProjectFileFromSet
    {
        /// <summary>
        /// Gets the original file entry that produced the file.
        /// </summary>
        FileSetEntry FileEntry { get; }

        /// <summary>
        /// Gets the root path of the set from which this file was originally loaded.
        /// </summary>
        string RootPath { get; }
    }
}
