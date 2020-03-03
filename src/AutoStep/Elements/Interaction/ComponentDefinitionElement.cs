using System;
using System.Collections.Generic;
using System.Text;
using AutoStep.Language;
using AutoStep.Language.Interaction.Parser;

namespace AutoStep.Elements.Interaction
{

    public class ComponentDefinitionElement : InteractionDefinitionElement
    {
        public List<NameRefElement> Traits { get; } = new List<NameRefElement>();

        public NameRefElement? Inherits { get; set; }
    }
}
