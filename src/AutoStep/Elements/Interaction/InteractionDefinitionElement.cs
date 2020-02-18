using System;
using System.Collections.Generic;
using System.Text;
using AutoStep.Language;
using AutoStep.Language.Interaction.Parser;

namespace AutoStep.Elements.Interaction
{
    public abstract class InteractionDefinitionElement : BuiltElement
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string? SourceName { get; set; }

        public List<MethodDefinitionElement> Methods { get; } = new List<MethodDefinitionElement>();

        public List<InteractionStepDefinitionElement> Steps { get; } = new List<InteractionStepDefinitionElement>();
    }
}
