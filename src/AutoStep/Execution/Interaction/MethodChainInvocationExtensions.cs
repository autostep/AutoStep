using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using AutoStep.Elements.Interaction;
using AutoStep.Execution.Contexts;
using AutoStep.Execution.Control;
using AutoStep.Language.Interaction;
using Microsoft.Extensions.DependencyInjection;

namespace AutoStep.Execution.Interaction
{
    /// <summary>
    /// Provides the ability to execute the method chain defined against an <see cref="ICallChainSource"/>.
    /// </summary>
    internal static class MethodChainInvocationExtensions
    {
        /// <summary>
        /// Invokes the method chain for the <see cref="ICallChainSource"/>. Walks through each method, binding parameters and invoking the underlying code method, or
        /// the file-defined method.
        /// </summary>
        /// <param name="callSource">The source of the call chain.</param>
        /// <param name="stepScope">The current service scope.</param>
        /// <param name="context">The current method context.</param>
        /// <param name="methods">The known method table for the invoking component.</param>
        /// <param name="cancelToken">Cancellation token for the call chain.</param>
        /// <param name="callStack">The method call stack.</param>
        /// <returns>A task that completes when the chain has completed.</returns>
        public static async ValueTask InvokeChainAsync(this ICallChainSource callSource, ILifetimeScope stepScope, MethodContext context, MethodTable methods, CancellationToken cancelToken, Stack<MethodContext>? callStack = null)
        {
            if (callStack is null)
            {
                callStack = new Stack<MethodContext>();
            }

            // Resolve the interaction set.
            var interactionSet = stepScope.Resolve<IInteractionSet>();

            // Get the execution manager.
            var executionManager = stepScope.Resolve<IExecutionStateManager>();

            // Define a new set of variables for this call.

            // Go through the method chain.
            foreach (var method in callSource.Calls)
            {
                // Locate this method.
                if (methods.TryGetMethod(method.MethodName, out var foundMethod))
                {
                    // Check to see we're not looping.
                    if (callStack.Any(m => m.MethodDefinition == foundMethod))
                    {
                        // Circular reference.
                        throw new CircularInteractionMethodException(method, callStack);
                    }

                    var boundArguments = BindArguments(stepScope, method, context, interactionSet.Constants);

                    var newContext = new MethodContext(method, foundMethod, context.Variables, boundArguments)
                    {
                        ChainValue = context.ChainValue,
                    };

                    try
                    {
                        callStack.Push(newContext);

                        // TODO - Allow step through of the methods.
                        var haltInstruction = await executionManager.CheckforHalt(stepScope, newContext, TestThreadState.StartingInteractionMethod);

                        await foundMethod.InvokeAsync(stepScope, newContext, methods, callStack, cancelToken);

                        context.ChainValue = newContext.ChainValue;
                    }
                    finally
                    {
                        callStack.Pop();
                    }
                }
                else
                {
                    // Method not available; compilation + set builder should have caught this.
                    throw new LanguageEngineAssertException();
                }
            }
        }

        private static object?[] BindArguments(ILifetimeScope scope, MethodCallElement call, MethodContext callingContext, InteractionConstantSet constants)
        {
            var providedArgs = call.Arguments;

            if (providedArgs.Count == 0)
            {
                return Array.Empty<object>();
            }

            var resultArray = new object?[providedArgs.Count];

            for (var argIdx = 0; argIdx < providedArgs.Count; argIdx++)
            {
                // Get the corresponding argument from the bound set.
                var callArg = providedArgs[argIdx];

                // Bind differently depending on the provided argument type.
                object? actualValue = callArg switch
                {
                    StringMethodArgumentElement strArg => strArg.GetFullText(scope, callingContext),
                    IntMethodArgumentElement intArg => intArg.Value,
                    FloatMethodArgumentElement floatArg => floatArg.Value,
                    VariableRefMethodArgumentElement varRefArg => callingContext.Variables.Get(varRefArg.VariableName),
                    VariableArrayRefMethodArgument _ => throw new NotImplementedException(),
                    ConstantMethodArgumentElement constantArg => constants.GetConstantValue(constantArg.ConstantName),
                    _ => throw new LanguageEngineAssertException()
                };

                resultArray[argIdx] = actualValue;
            }

            return resultArray;
        }
    }
}
