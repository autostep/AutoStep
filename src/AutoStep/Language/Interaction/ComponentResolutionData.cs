using System;
using System.Collections.Generic;
using System.Linq;
using AutoStep.Definitions.Interaction;
using AutoStep.Elements.Interaction;
using AutoStep.Language.Interaction.Traits;

namespace AutoStep.Language.Interaction
{
    internal class ComponentResolutionData
    {
        private bool createdOwnSteps;

        public ComponentResolutionData(string id)
        {
            Id = id;
        }

        public List<ComponentDefinitionElement> AllComponents { get; } = new List<ComponentDefinitionElement>();

        public bool Visited { get; set; }

        public string Id { get; }

        public string Name { get; set; }

        public ComponentDefinitionElement? FinalComponent { get; set; }

        public Dictionary<string, MethodDefinitionElement>? Methods { get; set; }

        public List<InteractionStepDefinitionElement>? Steps { get; set; }

        public IReadOnlyList<NameRefElement>? Traits { get; set; }

        public void Replace(ComponentDefinitionElement definition)
        {
            Methods = definition.Methods.ToDictionary(x => x.Name);
            Steps = definition.Steps;
            Traits = definition.Traits;
            Name = definition.Name;
            FinalComponent = definition;
        }

        public void ReplaceInheritance(ComponentResolutionData baseData, ComponentDefinitionElement derivedDefinition)
        {
            if (baseData.Methods is object)
            {
                Methods = new Dictionary<string, MethodDefinitionElement>(baseData.Methods);
            }
            else
            {
                Methods = null;
            }

            if (baseData.Steps is object)
            {
                Steps = baseData.Steps;
            }
            else
            {
                Steps = null;
            }

            Traits = baseData.Traits;

            Merge(derivedDefinition);
        }

        public void Merge(ComponentDefinitionElement definition)
        {
            // Method replacements.
            if (Methods is null)
            {
                if (definition.Methods.Any())
                {
                    Methods = definition.Methods.ToDictionary(x => x.Name);
                }
            }
            else if (definition.Methods.Any())
            {
                foreach (var def in definition.Methods)
                {
                    Methods[def.Name] = def;
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
                // Traits aren't merged, they are simply added.
                Traits = definition.Traits;
            }

            Name = definition.Name;
            FinalComponent = definition;
        }
    }
}
