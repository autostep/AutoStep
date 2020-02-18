using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Autofac.Features.ResolveAnything;
using AutoStep.Definitions;
using AutoStep.Elements.Interaction;
using AutoStep.Elements.Parts;
using AutoStep.Language.Interaction.Parser;
using AutoStep.Language.Interaction.Traits;

namespace AutoStep.Language.Interaction
{

    internal class AutoStepInteractionSet
    {
        private readonly Dictionary<string, BuiltComponent> components;
        private readonly IEnumerable<InteractionStepDefinitionElement> steps;

        public AutoStepInteractionSet(InteractionConstantSet constants, Dictionary<string, BuiltComponent> components, IEnumerable<InteractionStepDefinitionElement> steps)
        {
            this.components = components;
            this.Constants = constants;
            this.steps = steps;
        }

        public IReadOnlyDictionary<string, BuiltComponent> Components => components;

        public IEnumerable<StepDefinition> GetStepDefinitions(IStepDefinitionSource stepSource)
        {
            return steps.Select(s => new InteractionStepDefinition(stepSource, s));
        }

        public InteractionConstantSet Constants { get; }
  }
}
