using System.Collections.Generic;
using System.Threading.Tasks;
using AutoStep.Elements.Interaction;
using AutoStep.Execution.Dependency;
using AutoStep.Execution.Interaction;
using AutoStep.Language.Interaction;

namespace AutoStep.Definitions.Interaction
{

    public class FileDefinedInteractionMethod : InteractionMethod
    {
        public FileDefinedInteractionMethod(string name)
            : base(name)
        {
        }

        public bool NeedsDefining { get; set; }

        public MethodDefinitionElement MethodDefinition { get; set; }

        public override int ArgumentCount => MethodDefinition.Arguments.Count;

        public override async ValueTask InvokeAsync(IServiceScope scope, MethodContext context, object[] arguments, MethodTable methods, Stack<MethodContext> callStack)
        {
            BindArguments(context, arguments);

            // Invoke the method chain with the new context.
            await MethodDefinition.InvokeChainAsync(scope, context, methods, callStack);
        }

        private void BindArguments(MethodContext context, object[] arguments)
        {
            // Last chance catch for the wrong number of arguments. Compiler should have caught this.
            if (ArgumentCount != arguments.Length)
            {
                throw new LanguageEngineAssertException();
            }

            for (var argIdx = 0; argIdx < arguments.Length; argIdx++)
            {
                var methodArg = MethodDefinition.Arguments[argIdx];

                context.Variables.Set(methodArg.Name, arguments[argIdx]);
            }
        }
    }
}
