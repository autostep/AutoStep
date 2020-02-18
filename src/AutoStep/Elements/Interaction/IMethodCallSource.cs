using System;
using System.Collections.Generic;
using System.Text;
using AutoStep.Language.Interaction;

namespace AutoStep.Elements.Interaction
{
    internal interface IMethodCallSource
    {
        public string? SourceName { get; }

        public List<MethodCallElement> MethodCallChain { get; }

        InteractionMethodChainVariables GetInitialMethodChainVariables();
    }
}
