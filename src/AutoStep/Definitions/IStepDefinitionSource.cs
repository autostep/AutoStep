using System;
using System.Collections.Generic;
using AutoStep.Execution;

namespace AutoStep.Definitions
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
        /// Gets the step definitions available in the source.
        /// </summary>
        /// <returns>The step definitions.</returns>
        IEnumerable<StepDefinition> GetStepDefinitions();

        void RegisterExecutionServices(IServicesBuilder servicesBuilder);
    }
}
