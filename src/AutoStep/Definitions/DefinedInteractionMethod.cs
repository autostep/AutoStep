using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using AutoStep.Elements.Interaction;
using AutoStep.Execution;
using AutoStep.Execution.Binding;
using AutoStep.Execution.Dependency;
using AutoStep.Execution.Interaction;
using AutoStep.Language.Interaction;
using AutoStep.Language.Interaction.Parser;

namespace AutoStep.Definitions
{
    public abstract class DefinedInteractionMethod : InteractionMethod
    {
        private readonly MethodInfo method;

        public DefinedInteractionMethod(string name, MethodInfo method)
            : base(name)
        {
            this.method = method;
        }

        public override int ArgumentCount => method.GetParameters()
                                                   .Count(arg => !typeof(MethodContext).IsAssignableFrom(arg.ParameterType) &&
                                                                 !typeof(IServiceScope).IsAssignableFrom(arg.ParameterType));

        public override async ValueTask InvokeAsync(IServiceScope scope, MethodContext context, object[] arguments, MethodTable methods, Stack<MethodContext> callStack)
        {
            // Bind the arguments.
            var boundArguments = BindArguments(scope, arguments, context);

            // Get the target.
            var target = GetMethodTarget(scope);

            // Invoke.
            await InvokeInstanceMethod(target, boundArguments);
        }

        /// <summary>
        /// Resolves an instance of the owning type.
        /// </summary>
        /// <param name="scope">The current scope to resolve from.</param>
        /// <returns>An instance of the task.</returns>
        protected abstract object GetMethodTarget(IServiceScope scope);

        /// <summary>
        /// Invoke an instance method, generating a task wrapper if needed.
        /// </summary>
        /// <param name="target">The target object.</param>
        /// <param name="args">The method arguments.</param>
        /// <returns>A task that will completed when the method exits.</returns>
        private ValueTask InvokeInstanceMethod(object target, object[] args)
        {
            if (typeof(ValueTask).IsAssignableFrom(method.ReturnType))
            {
                return (ValueTask)method.Invoke(target, args);
            }
            else if (typeof(Task).IsAssignableFrom(method.ReturnType))
            {
                // This is an async method.
                var taskResult = (Task)method.Invoke(target, args);

                // Returning task directly, we don't need to do anything else with it here.
                return new ValueTask(taskResult);
            }
            else
            {
                method.Invoke(target, args);

                return default;
            }
        }

        private object[] BindArguments(IServiceScope scope, object[] providedArgs, MethodContext methodContext)
        {
            var methodArgs = method.GetParameters();

            if (methodArgs.Length == 0)
            {
                return Array.Empty<object>();
            }

            // Get the argument bind registry.
            var binderRegistry = scope.Resolve<ArgumentBinderRegistry>();
            var bindResult = new object[methodArgs.Length];
            var sourceArgPosition = 0;

            for (var argIdx = 0; argIdx < methodArgs.Length; argIdx++)
            {
                var arg = methodArgs[argIdx];

                if (arg.ParameterType.IsAssignableFrom(typeof(IServiceScope)))
                {
                    bindResult[argIdx] = scope;
                }
                else if (arg.ParameterType.IsAssignableFrom(typeof(MethodContext)))
                {
                    bindResult[argIdx] = methodContext;
                }
                else if (sourceArgPosition < providedArgs.Length)
                {
                    // Get the corresponding argument from the bound set.
                    var bindingArg = providedArgs[sourceArgPosition];
                    sourceArgPosition++;

                    bindResult[argIdx] = BindArgument(bindingArg, arg.ParameterType, scope, binderRegistry);
                }
            }

            return bindResult;
        }

        private object BindArgument(object argumentValue, Type parameterType, IServiceScope scope, ArgumentBinderRegistry registry)
        {
            object result;

            if (parameterType.IsAssignableFrom(argumentValue.GetType()))
            {
                // If assignable, just directly use the value.
                result = argumentValue;
            }
            else if (argumentValue is string strValue)
            {
                // String values should go through the argument binder.
                var binder = registry.GetBinderForType(scope, parameterType);

                try
                {
                    result = binder.Bind(strValue, parameterType);
                }
                catch (Exception ex)
                {
                    throw new ArgumentBindingException(strValue, parameterType, ex);
                }
            }
            else if (parameterType.IsValueType)
            {
                try
                {
                    // Try converting.
                    result = Convert.ChangeType(argumentValue, parameterType, CultureInfo.CurrentCulture);
                }
                catch (Exception ex)
                {
                    // Cannot use the value.
                    throw new InvalidCastException("Cannot use provided value '{0}' as an argument to {1}. Cannot convert to {2}.".FormatWith(argumentValue.ToString(), Name, parameterType.Name), ex);
                }
            }
            else
            {
                // Cannot directly use this value. Throw.
                throw new InvalidCastException("Cannot use provided value '{0}' as an argument to {1}. Cannot cast to expected type {2}.".FormatWith(argumentValue.ToString(), Name, parameterType.Name));
            }

            return result;
        }
    }
}
