using System;
using System.Collections.Generic;
using AutoStep.Definitions;
using AutoStep.Definitions.Test;
using AutoStep.Elements;
using AutoStep.Language;
using AutoStep.Language.Test;

namespace AutoStep.Projects
{
    /// <summary>
    /// Represents a single file within a project.
    /// </summary>
    public class ProjectTestFile : ProjectFile
    {
        private List<IUpdatableStepDefinitionSource>? dependencies;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectTestFile"/> class.
        /// </summary>
        /// <param name="filePath">The path to the file (might be physical or not, this is the identifier for the file).</param>
        /// <param name="contentSource">The content source used to access the raw file content.</param>
        public ProjectTestFile(string filePath, IContentSource contentSource)
            : base(filePath, contentSource)
        {
            LastCompileTime = DateTime.MinValue;
            LastLinkTime = DateTime.MinValue;
        }

        /// <summary>
        /// Gets the result of the last compilation operation on this project file.
        /// </summary>
        public FileCompilerResult? LastCompileResult { get; private set; }

        /// <summary>
        /// Gets the timestamp (UTC) of the last compilation result (successful or otherwise).
        /// </summary>
        public DateTime LastCompileTime { get; private set; }

        /// <summary>
        /// Gets the result of the last link operation on this project file.
        /// </summary>
        public LinkResult? LastLinkResult { get; private set; }

        /// <summary>
        /// Gets the timestamp (UTC) of the last link result (successful or otherwise).
        /// </summary>
        public DateTime? LastLinkTime { get; private set; }

        /// <summary>
        /// Gets the set of updatable step sources that the linked content of this file depends on. When those step sources change, this file will be re-linked.
        /// </summary>
        public IReadOnlyList<IUpdatableStepDefinitionSource>? LinkerDependencies => dependencies;

        /// <summary>
        /// Gets the step definition source for any step definitions defined in this file.
        /// </summary>
        internal FileStepDefinitionSource? StepDefinitionSource { get; private set; }

        /// <summary>
        /// Update this file with the latest compilation result.
        /// </summary>
        /// <param name="result">The result to update from.</param>
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

        /// <summary>
        /// Update this file with the latest link result.
        /// </summary>
        /// <param name="linkResult">The result to update from.</param>
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
