using System;
using System.Collections.Generic;
using AutoStep.Compiler;
using AutoStep.Definitions;

namespace AutoStep
{

    /// <summary>
    /// Represents a single file within a project.
    /// </summary>
    public class ProjectFile
    {
        private List<IUpdatableStepDefinitionSource>? dependencies;

        public ProjectFile(string filePath, IContentSource contentSource)
        {
            Path = filePath;
            ContentSource = contentSource;
            LastCompileTime = DateTime.MinValue;
            LastLinkTime = DateTime.MinValue;
        }

        public string Path { get; }

        public IContentSource ContentSource { get; }

        public FileCompilerResult? LastCompileResult { get; private set; }

        public DateTime LastCompileTime { get; private set; }

        public LinkResult? LastLinkResult { get; private set; }

        public DateTime? LastLinkTime { get; private set; }

        public bool IsAttachedToProject { get; set; }

        internal FileStepDefinitionSource? StepDefinitionSource { get; private set; }

        public void UpdateLastCompileResult(FileCompilerResult result)
        {
            LastCompileResult = result ?? throw new ArgumentNullException(nameof(result));
            LastCompileTime = DateTime.UtcNow;

            // If there are any step definitions in the file, update the associated source.
            if (result.Output?.StepDefinitions is IReadOnlyList<StepDefinition> defs && defs.Count > 0)
            {
                // There are some step definitions.
                if (StepDefinitionSource is null)
                {
                    StepDefinitionSource = new FileStepDefinitionSource(this);
                }
            }
        }

        public void UpdateLinkerDependencies(IEnumerable<IUpdatableStepDefinitionSource> newDependencies)
        {
            if (dependencies is null)
            {
                dependencies = new List<IUpdatableStepDefinitionSource>();
            }

            dependencies.Clear();
            dependencies.AddRange(newDependencies);
        }

        public IReadOnlyList<IUpdatableStepDefinitionSource>? LinkerDependencies => dependencies;
    }
}
