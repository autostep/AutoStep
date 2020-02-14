using System;
using System.Collections.Generic;
using System.Text;
using AutoStep.Language;
using AutoStep.Language.Interaction.Parser;

namespace AutoStep.Elements.Interaction
{

    public class MethodDefinitionElement : PositionalElement, IMethodCallSource
    {
        public string Name { get; set; }

        public bool IsUndefined { get; set; }

        public List<MethodCallElement> MethodCallChain { get; } = new List<MethodCallElement>();

        public List<MethodDefinitionArgumentElement> Arguments { get; } = new List<MethodDefinitionArgumentElement>();
    }

}
