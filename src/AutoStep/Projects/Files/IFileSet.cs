using System.Collections.Generic;

namespace AutoStep.Projects.Files
{
    public interface IFileSet
    {
        IReadOnlyList<FileSetEntry> Files { get; }

        string RootFolder { get; }
    }
}
