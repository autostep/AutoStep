using System;
using System.Collections.Generic;
using System.Text;
using AutoStep.Elements.Parts;
using AutoStep.Language;
using AutoStep.Language.Interaction;
using AutoStep.Language.Interaction.Parser;

namespace AutoStep.Elements.Interaction
{
    public class InteractionStepDefinitionElement : StepDefinitionElement, IMethodCallSource
    {
        private readonly HashSet<string> allComponents = new HashSet<string>();

        public List<MethodCallElement> MethodCallChain { get; } = new List<MethodCallElement>();

        public string? SourceName { get; set; }

        public string? FixedComponentName { get; set; }

        public InteractionMethodChainVariables GetInitialMethodChainVariables()
        {
            var variables = new InteractionMethodChainVariables();

            foreach (var item in Arguments)
            {
                variables.SetVariable(item.Name, false);
            }

            return variables;
        }

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

        public void AddComponentMatch(string componentName)
        {
            foreach (var part in Parts)
            {
                if (part is PlaceholderMatchPart compPart && compPart.PlaceholderValueName == InteractionPlaceholders.Component)
                {
                    compPart.MatchValue(componentName);
                }
            }

            allComponents.Add(componentName);
        }

        public bool MatchesSameComponentsAs(InteractionStepDefinitionElement otherElement)
        {
            return allComponents.SetEquals(otherElement.allComponents);
        }
    }
}
