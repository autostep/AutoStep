using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AutoStep.Execution;
using AutoStep.Execution.Dependency;

namespace AutoStep.Definitions
{
    /// <summary>
    /// Represents a source of step definitions backed by registered callbacks.
    /// </summary>
    public class CallbackDefinitionSource : IStepDefinitionSource
    {
        private readonly List<DelegateBackedStepDefinition> stepDefs = new List<DelegateBackedStepDefinition>();

        /// <summary>
        /// Gets the unique identifier for the source.
        /// </summary>
        public string Uid { get; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Gets the name of the source.
        /// </summary>
        public string Name => "Callbacks";

        /// <summary>
        /// Called before any tests execute to allow the source to register its own services to be resolved.
        /// </summary>
        /// <param name="servicesBuilder">The services builder.</param>
        /// <param name="configuration">The run-time configuration.</param>
        public void ConfigureServices(IServicesBuilder servicesBuilder, RunConfiguration configuration)
        {
            // No extra services.
        }

        /// <inheritdoc/>
        public IEnumerable<StepDefinition> GetStepDefinitions()
        {
            return stepDefs;
        }

        /// <summary>
        /// Add a delegate-backed step definition.
        /// </summary>
        /// <param name="stepDef">The definition.</param>
        public void Add(DelegateBackedStepDefinition stepDef)
        {
            stepDefs.Add(stepDef.ThrowIfNull(nameof(stepDef)));
        }
    }
}
