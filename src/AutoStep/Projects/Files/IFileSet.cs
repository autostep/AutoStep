using System.Collections.Generic;

namespace AutoStep.Projects.Files
{
    public interface IFileSet
    {
        IReadOnlyList<FileSetEntry> Files { get; }

        bool TryAddFile(string relativePath);

        bool TryRemoveFile(string relativePath);

        string RootFolder { get; }
    }
}
