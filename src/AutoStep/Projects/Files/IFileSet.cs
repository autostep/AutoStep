using System.Collections.Generic;

namespace AutoStep.Projects.Files
{
    /// <summary>
    /// Represents a globbed set of files found in a specific root directory.
    /// </summary>
    public interface IFileSet
    {
        /// <summary>
        /// Gets the set of found files.
        /// </summary>
        IReadOnlyList<FileSetEntry> Files { get; }

        /// <summary>
        /// Gets the root folder of the set.
        /// </summary>
        string RootFolder { get; }

        /// <summary>
        /// Try and add a file to the set, given the relative path. The add will only succeed if the file path matches the original globs for the set,
        /// and has not already been added. The file does not have to exist.
        /// </summary>
        /// <param name="relativePath">The relative path to the file (from the original set root folder).</param>
        /// <returns>True if the file path was successfully added to the set.</returns>
        bool TryAddFile(string relativePath);

        /// <summary>
        /// Try and remove a file from the set. The remove will only succeed if the file exists in the set.
        /// </summary>
        /// <param name="relativePath">The relative path to the file (as it was originally added).</param>
        /// <returns>True if the file was successfully removed from the set.</returns>
        bool TryRemoveFile(string relativePath);
    }
}
