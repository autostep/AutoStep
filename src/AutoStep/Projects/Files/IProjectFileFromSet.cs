namespace AutoStep.Projects.Files
{
    internal interface IProjectFileFromSet
    {
        FileSetEntry FileEntry { get; }

        string RootPath { get; }
    }
}
