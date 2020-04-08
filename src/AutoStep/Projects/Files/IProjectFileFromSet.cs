namespace AutoStep.Projects.Files
{
    public interface IProjectFileFromSet
    {
        FileSetEntry FileEntry { get; }

        string RootPath { get; }
    }
}
