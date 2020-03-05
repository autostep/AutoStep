using System;
using System.Reflection;
using System.Threading.Tasks;
using AutoStep.Execution;
using AutoStep.Execution.Binding;
using AutoStep.Execution.Contexts;
using AutoStep.Execution.Dependency;

namespace AutoStep.Definitions.Test
{
    /// <summary>
    /// Represents a step definition that calls a C# method of some form when the step is invoked.
    /// </summary>
    public abstract class MethodBackedStepDefinition : StepDefinition
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MethodBackedStepDefinition"/> class.
        /// </summary>
        /// <param name="source">The owning definition source.</param>
        /// <param name="method">The <see cref="MethodInfo"/> for the method.</param>
        /// <param name="type">The type of the step.</param>
        /// <param name="declaration">The text declaration of the step.</param>
        public MethodBackedStepDefinition(IStepDefinitionSource source, MethodInfo method, StepType type, string declaration)
            : base(source, type, declaration)
        {
            Method = method;
        }

        /// <summary>
        /// Gets the method info for the method to call.
        /// </summary>
        protected MethodInfo Method { get; }

        /// <summary>
        /// This method is invoked when the step definition should be executed.
        /// </summary>
        /// <param name="stepScope">The current DI scope.</param>
        /// <param name="context">The step context (including all binding information).</param>
        /// <param name="variables">The set of variables currently in-scope and available to the step.</param>
        /// <returns>A task that will complete when the step finishes executing.</returns>
        public override ValueTask ExecuteStepAsync(IServiceScope stepScope, StepContext context, VariableSet variables)
        {
            // Need to convert the found arguments into actual type arguments.
            var arguments = BindArguments(stepScope, context, variables);

            try
            {
                var target = GetMethodTarget(stepScope);

                return InvokeInstanceMethod(target, arguments);
            }
            catch (TargetInvocationException invokeEx)
            {
                // Unwrap this exception.
                if (invokeEx.InnerException is object)
                {
                    throw invokeEx.InnerException;
                }

                throw;
            }
        }

        /// <summary>
        /// Resolves an instance of the owning type.
        /// </summary>
        /// <param name="scope">The current scope to resolve from.</param>
        /// <returns>A task that completes when the method exits.</returns>
        protected abstract object GetMethodTarget(IServiceScope scope);

        /// <summary>
        /// Invoke an instance method, generating a task wrapper if needed.
        /// </summary>
        /// <param name="target">The target object.</param>
        /// <param name="args">The method arguments.</param>
        /// <returns>A task that will completed when the method exits.</returns>
        protected ValueTask InvokeInstanceMethod(object target, object[] args)
        {
            if (typeof(ValueTask).IsAssignableFrom(Method.ReturnType))
            {
                return (ValueTask)Method.Invoke(target, args);
            }
            else if (typeof(Task).IsAssignableFrom(Method.ReturnType))
            {
                // This is an async method.
                var taskResult = (Task)Method.Invoke(target, args);

                // Returning task directly, we don't need to do anything else with it here.
                return new ValueTask(taskResult);
            }
            else
            {
                Method.Invoke(target, args);

                return default;
            }
        }

        /// <summary>
        /// Bind the arguments in the given step context (and variables) to the method parameters.
        /// </summary>
        /// <param name="scope">The current service scope.</param>
        /// <param name="context">The step context.</param>
        /// <param name="variables">The currently available set of variables.</param>
        /// <returns>A bound set of arguments to pass to the method.</returns>
        protected object[] BindArguments(IServiceScope scope, StepContext context, VariableSet variables)
        {
            scope = scope.ThrowIfNull(nameof(scope));
            context = context.ThrowIfNull(nameof(context));
            variables = variables.ThrowIfNull(nameof(variables));

            if (context.Step.Binding is null)
            {
                throw new LanguageEngineAssertException();
            }

            var boundArgs = context.Step.Binding.Arguments;

            var methodArgs = Method.GetParameters().AsSpan();

            if (methodArgs.Length == 0)
            {
                // There are no arguments, don't bother, we'll just return an empty array.
                return Array.Empty<object>();
            }

            // Get the argument bind registry.
            var binderRegistry = scope.Resolve<ArgumentBinderRegistry>();
            var bindResult = new object[methodArgs.Length];
            var sourceArgPosition = 0;

            // Need to be sensible about the doc-string parameter here as well.
            for (var argIdx = 0; argIdx < methodArgs.Length; argIdx++)
            {
                var arg = methodArgs[argIdx];

                if (typeof(IServiceScope).IsAssignableFrom(arg.ParameterType))
                {
                    bindResult[argIdx] = scope;
                }
                else
                {
                    // Get the corresponding argument from the bound set.

                    // Note; we need to special-case the Table argument here.
                    var bindingArg = boundArgs[sourceArgPosition];
                    sourceArgPosition++;

                    var fullTextValue = bindingArg.GetFullText(scope, context.Step.Text, variables);

                    var binder = binderRegistry.GetBinderForType(scope, arg.ParameterType);

                    try
                    {
                        var boundValue = binder.Bind(fullTextValue, arg.ParameterType);

                        bindResult[argIdx] = boundValue;
                    }
                    catch (Exception ex)
                    {
                        throw new ArgumentBindingException(fullTextValue, arg.ParameterType, ex);
                    }
                }
            }

            return bindResult;
        }
    }
}
