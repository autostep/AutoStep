using System.Collections.Generic;
using System.Linq;
using AutoStep.Elements.Interaction;

namespace AutoStep.Language.Interaction
{
    /// <summary>
    /// Holds a temporary component resolution state while determining a component's inheritance chain.
    /// Used by the <see cref="InteractionSetBuilder"/> when it needs to determine a component's final version.
    /// </summary>
    internal class ComponentResolutionData
    {
        private bool createdOwnSteps;

        /// <summary>
        /// Initializes a new instance of the <see cref="ComponentResolutionData"/> class.
        /// </summary>
        /// <param name="id">The ID of the component.</param>
        public ComponentResolutionData(string id)
        {
            Id = id;
        }

        /// <summary>
        /// Gets the set of all component definitions that will be considered when resolving.
        /// </summary>
        public List<ComponentDefinitionElement> AllComponents { get; } = new List<ComponentDefinitionElement>();

        /// <summary>
        /// Gets the ID of the component.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Gets the currently selected 'name' of the component, which will be then used in trait step $component$ matching.
        /// </summary>
        public string? Name { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether the resolution process has already visited this component data.
        /// </summary>
        public bool Visited { get; set; }

        /// <summary>
        /// Gets the final determined component, that represents the bottom of the inheritance tree.
        /// </summary>
        public ComponentDefinitionElement? FinalComponent { get; private set; }

        /// <summary>
        /// Gets the set of all methods available from the fully resolved component.
        /// </summary>
        public Dictionary<string, MethodDefinitionElement>? Methods { get; private set; }

        /// <summary>
        /// Gets the set of all steps defined directly inside the fully resolved component.
        /// </summary>
        public List<InteractionStepDefinitionElement>? Steps { get; private set; }

        /// <summary>
        /// Gets the set of all traits associated to the resolved component.
        /// </summary>
        public IReadOnlyList<NameRefElement>? Traits { get; private set; }

        /// <summary>
        /// Replace the currently resolved component with a new one (equivalent to an override without an 'inherits' value).
        /// </summary>
        /// <param name="definition">The component definition.</param>
        public void Replace(ComponentDefinitionElement definition)
        {
            Methods = definition.Methods;
            Steps = definition.Steps;
            Traits = definition.Traits;
            Name = definition.Name;
            FinalComponent = definition;
        }

        /// <summary>
        /// With this method, we are entirely replacing the base set of component resolution data, then merging a new one in,
        /// i.e. 'rebasing' our component on top of another one.
        /// This happens when a component states that it inherits from another component ID, in which case the 'base' data is replaced
        /// with the 'other' component resolution data, and then we merge our definition on top.
        /// </summary>
        /// <param name="baseData">The base 'other' data.</param>
        /// <param name="derivedDefinition">The component to merge onto the base.</param>
        public void RebaseOnOtherComponent(ComponentResolutionData baseData, ComponentDefinitionElement derivedDefinition)
        {
            // Copy all the methods if there are any.
            if (baseData.Methods is object)
            {
                Methods = new Dictionary<string, MethodDefinitionElement>(baseData.Methods);
            }
            else
            {
                Methods = null;
            }

            // Reference the steps set if there are any (note this takes a reference to avoid duplication);
            // a subsequent merge can optionally replace this with a copy.
            Steps = baseData.Steps ?? null;

            // Traits are a direct overwrite.
            Traits = baseData.Traits;

            // Now merge our new definition on top of the base.
            Merge(derivedDefinition);
        }

        /// <summary>
        /// Merge a component definition element into this set of component resolution data.
        /// </summary>
        /// <param name="definition">The definition to merge in.</param>
        public void Merge(ComponentDefinitionElement definition)
        {
            // If there are no methods, then we just take a dictionary copy of the definition's methods.
            if (Methods is null)
            {
                if (definition.Methods.Any())
                {
                    Methods = definition.Methods;
                }
            }
            else if (definition.Methods.Any())
            {
                // Merge in the existing methods (will overwrite any existing definitions).
                foreach (var def in definition.Methods)
                {
                    Methods[def.Key] = def.Value;
                }
            }

            if (Steps is null)
            {
                if (definition.Steps.Any())
                {
                    Steps = definition.Steps;
                }
            }
            else if (definition.Steps.Any())
            {
                // Only copy the steps list when we merge in our own steps.
                if (!createdOwnSteps)
                {
                    // Now we need to copy the list.
                    Steps = new List<InteractionStepDefinitionElement>(Steps);
                    createdOwnSteps = true;
                }

                Steps.AddRange(definition.Steps);
            }

            if (definition.Traits.Any())
            {
                // Traits aren't merged, they are simply replaced.
                Traits = definition.Traits;
            }

            Name = definition.Name;
            FinalComponent = definition;
        }
    }
}
