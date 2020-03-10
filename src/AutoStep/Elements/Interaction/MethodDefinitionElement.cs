using System;
using System.Collections.Generic;
using System.Text;
using AutoStep.Language;
using AutoStep.Language.Interaction;
using AutoStep.Language.Interaction.Parser;

namespace AutoStep.Elements.Interaction
{

    public class MethodDefinitionElement : PositionalElement, ICallChainSource
    {
        public string Name { get; set; }

        public bool NeedsDefining { get; set; }

        public string? SourceName { get; set; }

        public List<MethodCallElement> Calls { get; } = new List<MethodCallElement>();

        public List<MethodDefinitionArgumentElement> Arguments { get; } = new List<MethodDefinitionArgumentElement>();

        public CallChainCompileTimeVariables GetCompileTimeChainVariables()
        {
            var variableSet = new CallChainCompileTimeVariables();

            foreach (var arg in Arguments)
            {
                variableSet.SetVariable(arg.Name, false);
            }

            return variableSet;
        }
    }

}
