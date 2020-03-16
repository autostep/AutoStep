using System.Collections.Generic;
using AutoStep.Definitions;
using AutoStep.Elements.Test;

namespace AutoStep.Language.Test
{
    /// <summary>
    /// Defines the interface for the AutoStep Linker, responsible for linking an individual file.
    /// </summary>
    public interface ILinker
    {
        /// <summary>
        /// Gets the set of all sources of step definitions registered with the linker.
        /// </summary>
        IEnumerable<IStepDefinitionSource> AllStepDefinitionSources { get; }

        /// <summary>
        /// Links (or re-links) a built autostep file; all step references that can be updated with step definition bindings will be.
        /// </summary>
        /// <param name="file">The file to link.</param>
        /// <returns>A link result (including a reference to the same file).</returns>
        LinkResult Link(FileElement file);

        /// <summary>
        /// Adds a source of step definitions to the linker (that won't change until its removed).
        /// </summary>
        /// <param name="source">The source to add.</param>
        void AddStepDefinitionSource(IStepDefinitionSource source);

        /// <summary>
        /// Add/Update an updatable step definition source. All step definitions in the source will be refreshed.
        /// </summary>
        /// <param name="stepDefinitionSource">The step definition source.</param>
        void AddOrUpdateStepDefinitionSource(IUpdatableStepDefinitionSource stepDefinitionSource);

        /// <summary>
        /// Remove a step definition source.
        /// </summary>
        /// <param name="stepDefinitionSource">The step definition source to remove.</param>
        void RemoveStepDefinitionSource(IStepDefinitionSource stepDefinitionSource);

        /// <summary>
        /// Bind a single step.
        /// </summary>
        /// <param name="stepReference">The step reference.</param>
        /// <param name="sourceName">The relevant source for any messages.</param>
        /// <param name="messages">An optional set to add messages to.</param>
        /// <returns>True if binding is successful, false otherwise.</returns>
        bool BindSingleStep(StepReferenceElement stepReference, string? sourceName = null, IList<LanguageOperationMessage>? messages = null);
    }
}
