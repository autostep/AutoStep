using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoStep.Elements.Interaction;
using AutoStep.Execution;
using AutoStep.Execution.Dependency;
using AutoStep.Execution.Interaction;
using AutoStep.Language.Interaction;

namespace AutoStep.Definitions.Interaction
{
    /// <summary>
    /// Represents an interaction method defined inside an interaction file.
    /// </summary>
    public class FileDefinedInteractionMethod : InteractionMethod
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileDefinedInteractionMethod"/> class.
        /// </summary>
        /// <param name="name">The method name.</param>
        /// <param name="definitionElement">The method definition element.</param>
        public FileDefinedInteractionMethod(string name, MethodDefinitionElement definitionElement)
            : base(name)
        {
            MethodDefinition = definitionElement;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the method needs defining (i.e. marked with 'needs-defining').
        /// </summary>
        public bool NeedsDefining { get; set; }

        /// <summary>
        /// Gets the method definition element.
        /// </summary>
        public MethodDefinitionElement MethodDefinition { get; }

        /// <summary>
        /// Gets or sets the overriden method that this method definition replaces.
        /// </summary>
        public InteractionMethod? OverriddenMethod { get; set; }

        /// <inheritdoc/>
        public override int ArgumentCount => MethodDefinition.Arguments.Count;

        /// <inheritdoc/>
        public override async ValueTask InvokeAsync(IServiceProvider scope, MethodContext context, object?[] arguments, MethodTable methods, Stack<MethodContext> callStack)
        {
            scope = scope.ThrowIfNull(nameof(scope));
            context = context.ThrowIfNull(nameof(context));
            arguments = arguments.ThrowIfNull(nameof(arguments));
            methods = methods.ThrowIfNull(nameof(methods));
            callStack = callStack.ThrowIfNull(nameof(callStack));

            // Need to use a custom method context for the subsequent method chain invocation, so that we can isolate the variables.
            var localContext = new MethodContext(context.MethodCall!, context.MethodDefinition!, new InteractionVariables())
            {
                ChainValue = context.ChainValue,
            };

            BindArguments(localContext, arguments);

            // Invoke the method chain with the new context.
            await MethodDefinition.InvokeChainAsync(scope, localContext, methods, callStack);

            // 'Return' the chain value from the local context.
            context.ChainValue = localContext.ChainValue;
        }

        private void BindArguments(MethodContext context, object?[] arguments)
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
