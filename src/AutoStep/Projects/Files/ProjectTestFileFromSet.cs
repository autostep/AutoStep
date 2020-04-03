using AutoStep.Language;

namespace AutoStep.Projects.Files
{
    internal class ProjectTestFileFromSet : ProjectTestFile, IProjectFileFromSet
    {
        public ProjectTestFileFromSet(string rootPath, FileSetEntry fileEntry, IContentSource contentSource)
            : base(fileEntry.Relative, contentSource)
        {
            RootPath = rootPath;
            FileEntry = fileEntry;
        }

        public FileSetEntry FileEntry { get; }

        public string RootPath { get; }
    }
}
