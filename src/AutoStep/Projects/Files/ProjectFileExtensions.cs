using System;
using System.Linq;
using AutoStep.Language;

namespace AutoStep.Projects.Files
{
    /// <summary>
    /// Extensions to merge file sets into projects.
    /// </summary>
    public static class ProjectFileExtensions
    {
        /// <summary>
        /// Merge a set that contains test files.
        /// </summary>
        /// <param name="project">The project.</param>
        /// <param name="fileSet">The file set.</param>
        /// <remarks>
        /// Any files in the project that were loaded from the same root dir, but are no longer in the file set, will be removed from the project.
        /// Files not already in the project will be added.
        /// </remarks>
        public static void MergeTestFileSet(this Project project, IFileSet fileSet)
        {
            project.MergeTestFileSet(fileSet, f => new PhysicalFileSource(f.Relative, f.Absolute));
        }

        /// <summary>
        /// Merge a set that contains test files, but providing a custom source.
        /// </summary>
        /// <param name="project">The project.</param>
        /// <param name="fileSet">The file set.</param>
        /// <param name="sourceFactory">A method to generate an <see cref="IContentSource"/> for a given file entry.</param>
        public static void MergeTestFileSet(this Project project, IFileSet fileSet, Func<FileSetEntry, IContentSource> sourceFactory)
        {
            if (project is null)
            {
                throw new ArgumentNullException(nameof(project));
            }

            if (fileSet is null)
            {
                throw new ArgumentNullException(nameof(fileSet));
            }

            MergeFileSet(project, fileSet, sourceFactory, (file, source) => new ProjectTestFileFromSet(fileSet.RootFolder, file, source));
        }

        /// <summary>
        /// Merge a set that contains interaction files.
        /// </summary>
        /// <param name="project">The project.</param>
        /// <param name="fileSet">The file set.</param>
        public static void MergeInteractionFileSet(this Project project, IFileSet fileSet)
        {
            project.MergeInteractionFileSet(fileSet, f => new PhysicalFileSource(f.Relative, f.Absolute));
        }

        /// <summary>
        /// Merge a set that contains interaction files.
        /// </summary>
        /// <param name="project">The project.</param>
        /// <param name="fileSet">The file set.</param>
        /// <param name="sourceFactory">A method to generate an <see cref="IContentSource"/> for a given file entry.</param>
        public static void MergeInteractionFileSet(this Project project, IFileSet fileSet, Func<FileSetEntry, IContentSource> sourceFactory)
        {
            if (project is null)
            {
                throw new ArgumentNullException(nameof(project));
            }

            if (fileSet is null)
            {
                throw new ArgumentNullException(nameof(fileSet));
            }

            MergeFileSet(project, fileSet, sourceFactory, (file, source) => new ProjectInteractionFileFromSet(fileSet.RootFolder, file, source));
        }

        private static void MergeFileSet<TFileType>(
            Project project,
            IFileSet fileSet,
            Func<FileSetEntry, IContentSource> sourceFactory,
            Func<FileSetEntry, IContentSource, TFileType> fileFactory)
            where TFileType : ProjectFile, IProjectFileFromSet
        {
            // Ok, so we need to merge the current set. Get the set of all current files that are of TFileType, in the same root folder.
            var currentFiles = project.AllFiles.OfType<TFileType>().Where(x => x.RootPath == fileSet.RootFolder).ToDictionary(i => i.FileEntry.Relative);

            foreach (var file in fileSet.Files)
            {
                // Try to remove this one from the set of known files.
                if (!currentFiles.Remove(file.Relative))
                {
                    // File does not exist, add it.
                    var source = sourceFactory(file);

                    var projectFile = fileFactory(file, source);

                    if (projectFile is ProjectTestFile test)
                    {
                        project.TryAddFile(test);
                    }
                    else if (projectFile is ProjectInteractionFile interaction)
                    {
                        project.TryAddFile(interaction);
                    }
                }
            }

            // Now remove any files that we didn't see.
            foreach (var toRemove in currentFiles)
            {
                project.TryRemoveFile(toRemove.Value);
            }
        }
    }
}
