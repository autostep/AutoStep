using System.Collections.Generic;

namespace AutoStep.Elements
{
    /// <summary>
    /// Defines the built content of an AutoStep content source (i.e. a raw file becomes a set of built content).
    /// </summary>
    public class CodeEntityElement : BuiltElement
    {
        private List<StepDefinitionElement>? stepDefinitions;

        /// <summary>
        /// Gets any general step definitions defined in the file.
        /// </summary>
        public IReadOnlyList<StepDefinitionElement>? StepDefinitions => stepDefinitions;

        /// <summary>
        /// Gets or sets the (optional feature) found in the file.
        /// </summary>
        public FeatureElement? Feature { get; set; }

        /// <summary>
        /// Gets the set of all step references within the content block.
        /// </summary>
        /// <remarks>
        /// Used for faster linking, by adding all step references at the top level.
        /// </remarks>
        public LinkedList<StepReferenceElement> AllStepReferences { get; } = new LinkedList<StepReferenceElement>();

        /// <summary>
        /// Add a step definition to the built content.
        /// </summary>
        /// <param name="definition">The step definition element.</param>
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
