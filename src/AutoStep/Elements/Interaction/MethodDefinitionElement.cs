using System;
using System.Collections.Generic;
using System.Text;
using AutoStep.Language;
using AutoStep.Language.Interaction;
using AutoStep.Language.Interaction.Parser;

namespace AutoStep.Elements.Interaction
{

    public class MethodDefinitionElement : PositionalElement, IMethodCallSource
    {
        public string Name { get; set; }

        public bool NeedsDefining { get; set; }

        public string? SourceName { get; set; }

        public List<MethodCallElement> MethodCallChain { get; } = new List<MethodCallElement>();

        public List<MethodDefinitionArgumentElement> Arguments { get; } = new List<MethodDefinitionArgumentElement>();

        public InteractionMethodChainVariables GetInitialMethodChainVariables()
        {
            var variableSet = new InteractionMethodChainVariables();

            foreach (var arg in Arguments)
            {
                variableSet.SetVariable(arg.Name, false);
            }

            return variableSet;
        }
    }

}
