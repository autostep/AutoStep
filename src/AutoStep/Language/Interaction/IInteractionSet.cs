using System.Collections.Generic;
using AutoStep.Definitions;

namespace AutoStep.Language.Interaction
{
    /// <summary>
    /// Defines an interaction set, defining the set of components available for interaction behaviour,
    /// and access to the set of step definitions.
    /// </summary>
    public interface IInteractionSet
    {
        /// <summary>
        /// Gets the set of all components, indexed by name.
        /// </summary>
        IReadOnlyDictionary<string, BuiltComponent> Components { get; }

        /// <summary>
        /// Gets the set of available constants.
        /// </summary>
        InteractionConstantSet Constants { get; }

        /// <summary>
        /// Gets the set of all step definitions.
        /// </summary>
        /// <param name="stepSource">The step source to attach them to.</param>
        /// <returns>A set of steps.</returns>
        IEnumerable<StepDefinition> GetStepDefinitions(IStepDefinitionSource stepSource);
    }
}
