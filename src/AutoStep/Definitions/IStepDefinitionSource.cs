using System.Collections.Generic;
using AutoStep.Execution;
using AutoStep.Execution.Dependency;

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

        /// <summary>
        /// Called before any tests execute to allow the source to register its own services to be resolved.
        /// </summary>
        /// <param name="servicesBuilder">The services builder.</param>
        /// <param name="configuration">The run-time configuration.</param>
        void ConfigureServices(IServicesBuilder servicesBuilder, RunConfiguration configuration);
    }
}
