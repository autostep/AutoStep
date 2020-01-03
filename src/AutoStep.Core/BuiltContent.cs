using System.Collections.Generic;
using AutoStep.Core.Elements;

namespace AutoStep.Core
{
    /// <summary>
    /// Defines the built content of an AutoStep content source (i.e. a raw file becomes a set of built content).
    /// </summary>
    public class BuiltContent : BuiltElement
    {
        private List<StepDefinitionElement> stepDefinitions;

        /// <summary>
        /// Gets any general step definitions defined in the file.
        /// </summary>
        public IReadOnlyList<StepDefinitionElement> StepDefinitions => stepDefinitions;

        /// <summary>
        /// Gets or sets the (optional feature) found in the file.
        /// </summary>
        public FeatureElement Feature { get; set; }

        public void AddStepDefinition(StepDefinitionElement definition)
        {
            if (definition is null)
            {
                throw new System.ArgumentNullException(nameof(definition));
            }

            if (stepDefinitions is null)
            {
                stepDefinitions = new List<StepDefinitionElement>();
            }

            stepDefinitions.Add(definition);
        }
    }
}
