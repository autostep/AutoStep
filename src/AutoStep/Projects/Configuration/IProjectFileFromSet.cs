namespace AutoStep.Projects.Configuration
{
    internal interface IProjectFileFromSet
    {
        FileSetEntry FileEntry { get; }

        string RootPath { get; }
    }
}
