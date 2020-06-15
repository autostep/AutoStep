using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using AutoStep.Execution;
using AutoStep.Execution.Dependency;
using AutoStep.Language.Interaction;
using Microsoft.Extensions.Configuration;

namespace AutoStep.Definitions.Interaction
{
    /// <summary>
    /// An interactions step definition source that holds all of the step definitions exposed by
    /// the interactions system.
    /// </summary>
    internal class InteractionStepDefinitionSource : IUpdatableStepDefinitionSource
    {
        private IInteractionSet? interactions;
        private List<StepDefinition>? cachedSteps;
        private DateTime lastModifyTime;

        /// <inheritdoc/>
        public string Uid => "interaction";

        /// <inheritdoc/>
        public string Name => "Interaction";

        /// <summary>
        /// Updates the steps from a new interaction set.
        /// </summary>
        /// <param name="interactions">The interaction set.</param>
        internal void UpdateInteractionSet(IInteractionSet interactions)
        {
            this.interactions = interactions;
            lastModifyTime = DateTime.UtcNow;
            cachedSteps = interactions.GetStepDefinitions(this).ToList();
        }

        /// <inheritdoc/>
        public void ConfigureServices(ContainerBuilder containerBuilder, IConfiguration configuration)
        {
            if (interactions is object)
            {
                // Register the interactions set, so we can retrieve it in individual steps.
                containerBuilder.ThrowIfNull(nameof(containerBuilder)).RegisterInstance(interactions);
            }
        }

        /// <inheritdoc/>
        public DateTime GetLastModifyTime()
        {
            return lastModifyTime;
        }

        /// <inheritdoc/>
        public IEnumerable<StepDefinition> GetStepDefinitions()
        {
            if (cachedSteps is object)
            {
                return cachedSteps;
            }

            // Step definitions are only available after a call to UpdateInteractionSet.
            return Enumerable.Empty<StepDefinition>();
        }
    }
}
