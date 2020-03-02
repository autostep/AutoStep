using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoStep.Definitions;
using AutoStep.Elements.Interaction;
using AutoStep.Execution.Contexts;
using AutoStep.Execution.Control;
using AutoStep.Execution.Dependency;
using AutoStep.Language.Interaction;
using AutoStep.Language.Interaction.Parser;

namespace AutoStep.Execution.Interaction
{
    internal static class MethodChainInvocationExtensions
    {
        public static async ValueTask InvokeChainAsync(this IMethodCallSource callSource, IServiceScope stepScope, MethodContext context, MethodTable methods, Stack<MethodContext>? callStack = null)
        {
            if (callStack is null)
            {
                callStack = new Stack<MethodContext>();
            }

            // Resolve the interaction set.
            var interactionSet = stepScope.Resolve<AutoStepInteractionSet>();

            // Get the execution manager.
            var executionManager = stepScope.Resolve<IExecutionStateManager>();

            // Go through the method chain.
            foreach (var method in callSource.MethodCallChain)
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

                    var newContext = new MethodContext(method, foundMethod);

                    newContext.ChainValue = context.ChainValue;

                    callStack.Push(newContext);

                    try
                    {
                        var boundArguments = BindArguments(stepScope, method, context, interactionSet.Constants);

                        // TODO - Allow step through of the methods.
                        var haltInstruction = await executionManager.CheckforHalt(stepScope, newContext, TestThreadState.StartingInteractionMethod);

                        await foundMethod.InvokeAsync(stepScope, newContext, boundArguments, methods, callStack);

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

        private static object[] BindArguments(IServiceScope scope, MethodCallElement call, MethodContext callingContext, InteractionConstantSet constants)
        {
            var providedArgs = call.Arguments;

            if (providedArgs.Count == 0)
            {
                return Array.Empty<object>();
            }

            var resultArray = new object[providedArgs.Count];

            for (var argIdx = 0; argIdx < providedArgs.Count; argIdx++)
            {
                // Get the corresponding argument from the bound set.
                var callArg = providedArgs[argIdx];

                // Bind differently depending on the provided argument type.
                object actualValue = callArg switch
                {
                    StringMethodArgumentElement strArg => strArg.GetFullText(scope, callingContext),
                    IntMethodArgumentElement intArg => intArg.Value,
                    FloatMethodArgument floatArg => floatArg.Value,
                    VariableRefMethodArgumentElement varRefArg => callingContext.Get<object>(varRefArg.VariableName),
                    VariableArrayRefMethodArgument _ => throw new NotImplementedException(),
                    ConstantMethodArgument constantArg => constants.GetConstantValue(constantArg.ConstantName),
                    _ => throw new LanguageEngineAssertException()
                };

                resultArray[argIdx] = actualValue;
            }

            return resultArray;
        }
    }
}
