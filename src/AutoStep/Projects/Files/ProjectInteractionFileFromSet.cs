using AutoStep.Language;

namespace AutoStep.Projects.Files
{
    internal class ProjectInteractionFileFromSet : ProjectInteractionFile, IProjectFileFromSet
    {
        public ProjectInteractionFileFromSet(string rootPath, FileSetEntry fileEntry, IContentSource contentSource)
            : base(fileEntry.Relative, contentSource)
        {
            RootPath = rootPath;
            FileEntry = fileEntry;
        }

        public FileSetEntry FileEntry { get; }

        public string RootPath { get; }
    }
}
