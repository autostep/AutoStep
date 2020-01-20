using System;
using System.Reflection;
using System.Threading.Tasks;
using AutoStep.Execution;
using AutoStep.Execution.Binding;
using AutoStep.Execution.Dependency;

namespace AutoStep.Definitions
{
    public abstract class MethodBackedStepDefinition : StepDefinition
    {
        public MethodBackedStepDefinition(IStepDefinitionSource source, MethodInfo method, StepType type, string declaration)
            : base(source, type, declaration)
        {
            Method = method;
        }

        protected MethodInfo Method { get; }

        public override Task ExecuteStepAsync(IServiceScope stepScope, StepContext context, VariableSet variables)
        {
            // TODO - Bind the step and execute!!

            // Need to convert the found arguments into actual type arguments.
            object[] arguments = BindArguments(stepScope, context, variables);

            try
            {
               return InvokeMethod(stepScope, arguments);
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

        protected abstract Task InvokeMethod(IServiceScope scope, object[] args);

        protected Task InvokeInstanceMethod(IServiceScope scope, object target, object[] args)
        {
            if (typeof(Task).IsAssignableFrom(Method.ReturnType))
            {
                // This is an async method.
                var taskResult = (Task)Method.Invoke(target, args);

                // Returning task directly, we don't need to do anything else with it here.
                return taskResult;
            }
            else if (typeof(ValueTask).IsAssignableFrom(Method.ReturnType))
            {
                var taskResult = (ValueTask)Method.Invoke(target, args);

                if (taskResult.IsCompleted)
                {
                    return Task.CompletedTask;
                }

                return taskResult.AsTask();
            }
            else
            {
                Method.Invoke(target, args);

                return Task.CompletedTask;
            }
        }

        protected object[] BindArguments(IServiceScope scope, StepContext context, VariableSet variables)
        {
            if (context.Step.Binding is null)
            {
                throw new LanguageEngineAssertException();
            }

            var boundArgs = context.Step.Binding.Arguments;
            var table = context.Step.Table;

            // Get the argument bind registry.
            var binderRegistry = scope.Resolve<ArgumentBinderRegistry>();

            var methodArgs = Method.GetParameters();

            var bindResult = new object[methodArgs.Length];

            if (methodArgs.Length != boundArgs.Length)
            {
                // For now we'll just do the basic argument case.
                throw new NotImplementedException();
            }

            // Need to be sensible about the doc-string parameter here as well.
            for (var argIdx = 0; argIdx < methodArgs.Length; argIdx++)
            {
                var arg = methodArgs[argIdx];

                // Get the corresponding argument from the bound set.

                // Note; we need to special-case the Table argument here.
                var bindingArg = boundArgs[argIdx];

                var fullTextValue = bindingArg.GetFullText(scope, context.Step.Text, variables);

                var binder = binderRegistry.GetBinderForType(scope, arg.ParameterType);

                var boundValue = binder.Bind(fullTextValue, arg.ParameterType);

                bindResult[argIdx] = boundValue;
            }

            return bindResult;
        }

    }
}
