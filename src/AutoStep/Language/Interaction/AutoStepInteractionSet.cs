using System.Collections.Generic;
using System.Linq;
using AutoStep.Definitions;
using AutoStep.Definitions.Interaction;
using AutoStep.Elements.Interaction;

namespace AutoStep.Language.Interaction
{
    /// <summary>
    /// Defines an interaction set, defining the set of components available for interaction behaviour,
    /// and access to the set of step definitions.
    /// </summary>
    internal class AutoStepInteractionSet
    {
        private readonly IEnumerable<InteractionStepDefinitionElement> steps;

        /// <summary>
        /// Initializes a new instance of the <see cref="AutoStepInteractionSet"/> class.
        /// </summary>
        /// <param name="constants">The set of available constants.</param>
        /// <param name="components">The set of all components, indexed by name.</param>
        /// <param name="steps">The set of all step definitions.</param>
        public AutoStepInteractionSet(InteractionConstantSet constants, IReadOnlyDictionary<string, BuiltComponent> components, IEnumerable<InteractionStepDefinitionElement> steps)
        {
            this.Components = components;
            this.Constants = constants;
            this.steps = steps;
        }

        /// <summary>
        /// Gets the set of all components, indexed by name.
        /// </summary>
        public IReadOnlyDictionary<string, BuiltComponent> Components { get; }

        /// <summary>
        /// Gets the set of available constants.
        /// </summary>
        public InteractionConstantSet Constants { get; }

        /// <summary>
        /// Gets the set of all step definitions.
        /// </summary>
        /// <param name="stepSource">The step source to attach them to.</param>
        /// <returns>A set of steps.</returns>
        public IEnumerable<StepDefinition> GetStepDefinitions(IStepDefinitionSource stepSource)
        {
            return steps.Select(s => new InteractionStepDefinition(stepSource, s));
        }
  }
}
