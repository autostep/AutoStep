using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AutoStep.Elements.Interaction;
using AutoStep.Execution.Dependency;
using AutoStep.Execution.Interaction;
using AutoStep.Language.Interaction;
using AutoStep.Language.Interaction.Parser;

namespace AutoStep.Definitions
{
    public abstract class InteractionMethod
    {
        protected InteractionMethod(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public virtual void ProvideUpdatedCompilationVariables(InteractionMethodChainVariables variables)
        {
        }

        // Todo: Figure out the parameters for this.
        public abstract ValueTask InvokeAsync(IServiceScope scope, MethodContext context, object[] arguments, MethodTable methods, Stack<MethodContext> callStack);

        public abstract int ArgumentCount { get; }
    }
}
