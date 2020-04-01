using System;
using System.Linq;
using AutoStep.Language;

namespace AutoStep.Projects.Configuration
{
    public static class ProjectExtensions
    {
        public static void MergeTestFileSet(this Project project, IFileSet fileSet)
        {
            project.MergeTestFileSet(fileSet, f => new PhysicalFileSource(f.Relative, f.Absolute));
        }

        public static void MergeTestFileSet(this Project project, IFileSet fileSet, Func<FileSetEntry, IContentSource> sourceFactory)
        {
            MergeFileSet(project, fileSet, sourceFactory, (file, source) => new ProjectTestFileFromSet(fileSet.RootFolder, file, source));
        }

        public static void MergeInteractionFileSet(this Project project, IFileSet fileSet)
        {
            project.MergeInteractionFileSet(fileSet, f => new PhysicalFileSource(f.Relative, f.Absolute));
        }

        public static void MergeInteractionFileSet(this Project project, IFileSet fileSet, Func<FileSetEntry, IContentSource> sourceFactory)
        {
            MergeFileSet(project, fileSet, sourceFactory, (file, source) => new ProjectInteractionFileFromSet(fileSet.RootFolder, file, source));
        }

        private static void MergeFileSet<TFileType>(
            Project project,
            IFileSet fileSet,
            Func<FileSetEntry, IContentSource> sourceFactory,
            Func<FileSetEntry, IContentSource, TFileType> fileFactory)
            where TFileType : ProjectFile, IProjectFileFromSet
        {
            // Ok, so we need to merge the current set. Get the set of all current files that are of TFileType.
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
