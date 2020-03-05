using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoStep.Elements.Interaction;
using AutoStep.Execution.Dependency;
using AutoStep.Execution.Interaction;
using AutoStep.Language.Interaction;

namespace AutoStep.Definitions.Interaction
{
    public abstract class InteractionMethod
    {
        protected InteractionMethod(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public virtual void CompilerMethodCall(IReadOnlyList<MethodArgumentElement> arguments, InteractionMethodChainVariables variables)
        {
        }

        public virtual ValueTask InvokeAsync(IServiceScope scope, MethodContext context, object[] arguments)
        {
            throw new NotImplementedException($"Registered Interaction Method '{Name}' has not been implemented");
        }

        public virtual ValueTask InvokeAsync(IServiceScope scope, MethodContext context, object[] arguments, MethodTable methods, Stack<MethodContext> callStack)
        {
            return InvokeAsync(scope, context, arguments);
        }

        public abstract int ArgumentCount { get; }
    }
}
