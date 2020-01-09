using System;
using System.Collections.Generic;
using AutoStep.Compiler;
using AutoStep.Definitions;
using AutoStep.Elements;

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
            if (filePath is null)
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            Path = filePath;
            ContentSource = contentSource ?? throw new ArgumentNullException(nameof(contentSource));
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

        public IReadOnlyList<IUpdatableStepDefinitionSource>? LinkerDependencies => dependencies;

        internal FileStepDefinitionSource? StepDefinitionSource { get; private set; }

        public void UpdateLastCompileResult(FileCompilerResult result)
        {
            LastCompileResult = result ?? throw new ArgumentNullException(nameof(result));
            LastCompileTime = DateTime.UtcNow;

            // If there are any step definitions in the file, update the associated source.
            if (result.Output?.StepDefinitions is IReadOnlyList<StepDefinitionElement> defs && defs.Count > 0)
            {
                // There are some step definitions.
                if (StepDefinitionSource is null)
                {
                    StepDefinitionSource = new FileStepDefinitionSource(this);
                }
            }
        }

        public void UpdateLastLinkResult(LinkResult linkResult)
        {
            LastLinkResult = linkResult ?? throw new ArgumentNullException(nameof(linkResult));
            LastLinkTime = DateTime.UtcNow;

            if (dependencies is null)
            {
                dependencies = new List<IUpdatableStepDefinitionSource>();
            }
            else
            {
                dependencies.Clear();
            }

            // Check for any file sources.
            foreach (var refSource in linkResult.ReferencedSources)
            {
                if (refSource is IUpdatableStepDefinitionSource updatableSource)
                {
                    // The source is another file in the project; add it as a referenced step.
                    dependencies.Add(updatableSource);
                }
            }
        }
    }
}
