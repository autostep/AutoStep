using System;
using System.Collections.Generic;

namespace AutoStep.Sources
{
    /// <summary>
    /// Defines a source of step definitions.
    /// </summary>
    public interface IStepDefinitionSource
    {
        /// <summary>
        /// Gets a unique, non-human-readable identifier for the source. Two sources with the same UID cannot share steps.
        /// </summary>
        string Uid { get; }

        /// <summary>
        /// Gets a human-readable name for the source.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets a value indicating the last time the source was modified.
        /// </summary>
        /// <returns>The last-modify timestamp.</returns>
        DateTime GetLastModifyTime();

        /// <summary>
        /// Gets the step definitions available in the source.
        /// </summary>
        /// <returns>The step definitions.</returns>
        IEnumerable<StepDefinition> GetStepDefinitions();
    }
}
