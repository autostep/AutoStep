using System.Collections.Generic;

namespace AutoStep.Projects.Configuration
{
    public interface IFileSet
    {
        IReadOnlyList<FileSetEntry> Files { get; }

        string RootFolder { get; }
    }
}
