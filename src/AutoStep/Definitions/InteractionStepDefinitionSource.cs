using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoStep.Execution;
using AutoStep.Execution.Dependency;
using AutoStep.Language.Interaction;

namespace AutoStep.Definitions
{
    public class InteractionStepDefinitionSource : IUpdatableStepDefinitionSource
    {
        private AutoStepInteractionSet? interactions;
        private List<StepDefinition>? cachedSteps;
        private DateTime lastModifyTime;

        public string Uid => "interaction";

        public string Name => "Interaction";

        internal void UpdateInteractionSet(AutoStepInteractionSet interactions)
        {
            this.interactions = interactions;
            lastModifyTime = DateTime.UtcNow;
            cachedSteps = interactions.GetStepDefinitions(this).ToList();
        }

        public void ConfigureServices(IServicesBuilder servicesBuilder, RunConfiguration configuration)
        {
            if (interactions is object)
            {
                // Register the interactions set, so we can retrieve it in individual steps.
                servicesBuilder.ThrowIfNull(nameof(servicesBuilder)).RegisterSingleInstance(interactions);                
            }
        }

        public DateTime GetLastModifyTime()
        {
            return lastModifyTime;
        }

        public IEnumerable<StepDefinition> GetStepDefinitions()
        {
            if (cachedSteps is object)
            {
                return cachedSteps;
            }

            return Enumerable.Empty<StepDefinition>();
        }
    }
}
