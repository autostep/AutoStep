using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoStep.Elements.Parts;
using AutoStep.Execution.Interaction;
using AutoStep.Language;
using AutoStep.Language.Interaction;

namespace AutoStep.Elements.Interaction
{
    /// <summary>
    /// Represents a step definition element defined in an interactions file.
    /// </summary>
    public class InteractionStepDefinitionElement : StepDefinitionElement, ICallChainSource
    {
        private readonly HashSet<string> allComponents = new HashSet<string>();

        /// <summary>
        /// Gets the set of calls in the call chain this step invokes.
        /// </summary>
        public List<MethodCallElement> Calls { get; } = new List<MethodCallElement>();

        /// <summary>
        /// Gets or sets the source name of the file containing this element.
        /// </summary>
        public string? SourceName { get; set; }

        /// <summary>
        /// Gets or sets a fixed 'known' component name for this step definition. This
        /// will be set for steps defined in components, but not for steps defined in traits.
        /// </summary>
        public string? FixedComponentName { get; set; }

        /// <inheritdoc/>
        public CallChainCompileTimeVariables GetCompileTimeChainVariables()
        {
            var variables = new CallChainCompileTimeVariables();

            foreach (var item in Arguments)
            {
                variables.SetVariable(item.Name, false);
            }

            return variables;
        }

        /// <summary>
        /// Empties the step of all component matching data.
        /// </summary>
        public void ClearAllComponentMatchData()
        {
            foreach (var part in Parts)
            {
                if (part is PlaceholderMatchPart compPart)
                {
                    compPart.ClearAllValues();
                }
            }

            allComponents.Clear();
        }

        /// <summary>
        /// Adds a component match to the step definition (so that this step definition will match a given component).
        /// </summary>
        /// <param name="componentName">The name of the component.</param>
        public void AddComponentMatch(string componentName)
        {
            // Each placeholder matching part needs to include this value.
            foreach (var part in Parts)
            {
                if (part is PlaceholderMatchPart compPart && compPart.PlaceholderValueName == StepPlaceholders.Component)
                {
                    compPart.AddMatchingValue(componentName);
                }
            }

            allComponents.Add(componentName);
        }

        /// <summary>
        /// Gets the set of valid components.
        /// </summary>
        public IEnumerable<string> ValidComponents => allComponents;

        /// <summary>
        /// Gets a step definition signature.
        /// </summary>
        /// <returns>The signature.</returns>
        internal InteractionStepSignature GetSignature()
        {
            return new InteractionStepSignature(Type, Declaration!, allComponents);
        }

        /// <summary>
        /// Gets a step definition signature for the element that only includes the declaration body.
        /// </summary>
        /// <returns>The signature object.</returns>
        internal StepDeclarationSignature GetDeclarationOnlySignature()
        {
            return new StepDeclarationSignature(Type, Declaration!);
        }

        /// <summary>
        /// Checks whether this element matches the same set of components as the other element.
        /// </summary>
        /// <param name="otherElement">The other element.</param>
        /// <returns>True if all of the two elements have the same components.</returns>
        public bool MatchesSameComponentsAs(InteractionStepDefinitionElement otherElement)
        {
            if (otherElement is null)
            {
                throw new System.ArgumentNullException(nameof(otherElement));
            }

            return allComponents.SetEquals(otherElement.allComponents);
        }
    }
}
