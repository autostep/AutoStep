using System;
using System.Collections.Generic;
using System.Text;
using AutoStep.Language;
using AutoStep.Language.Interaction.Parser;

namespace AutoStep.Elements.Interaction
{

    public class MethodDefinitionArgumentElement : PositionalElement
    {
        public string Name { get; set; }

        public string TypeHint { get; set; }
    }

}
