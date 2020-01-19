using System;
using System.Reflection;
using System.Threading.Tasks;
using AutoStep.Execution;
using AutoStep.Execution.Binding;

namespace AutoStep.Definitions
{
    /// <summary>
    /// Represents a step definition backed by a method in code.
    /// </summary>
    public class BuiltStepDefinition : StepDefinition
    {
        private readonly Type owner;
        private readonly MethodInfo method;

        /// <summary>
        /// Initializes a new instance of the <see cref="BuiltStepDefinition"/> class.
        /// </summary>
        /// <param name="source">The source used to load the step definition.</param>
        /// <param name="method">The method info representing the method to bind against.</param>
        /// <param name="declaringAttribute">The attribute that declared the step definition.</param>
        public BuiltStepDefinition(AssemblyStepDefinitionSource source, Type owner, MethodInfo method, StepDefinitionAttribute declaringAttribute)
            : base(source, declaringAttribute?.Type ?? throw new ArgumentNullException(nameof(declaringAttribute)), declaringAttribute.Declaration)
        {
            this.owner = owner;
            this.method = method;
        }

        public override async Task ExecuteStepAsync(StepExecutionArgs executionArguments)
        {
            if (executionArguments is null)
            {
                throw new ArgumentNullException(nameof(executionArguments));
            }

            // TODO - Bind the step and execute!!

            // Need to convert the found arguments into actual type arguments.
            object[] arguments = BindArguments(executionArguments);

            // Resolve an instance of the service. It will let it access any services.
            var instance = executionArguments.Scope.Resolve<object>(owner);

            try
            {
                method.Invoke(instance, arguments);
            }
            catch (TargetInvocationException invokeEx)
            {
                // Unwrap this exception.
                if (invokeEx.InnerException is object)
                {
                    throw invokeEx.InnerException;
                }
            }
        }

        private object[] BindArguments(StepExecutionArgs executionArguments)
        {
            var boundArgs = executionArguments.Binding.Arguments;
            var table = executionArguments.Step.Table;

            // Get the argument bind registry.
            executionArguments.Scope.Resolve<ArgumentBinderRegistry>();

            var methodArgs = method.GetParameters();

            var bindResult = new object[methodArgs.Length];

            for (var argIdx = 0; argIdx < methodArgs.Length; argIdx++)
            {
                var arg = methodArgs[argIdx];

                // Get the corresponding argument from the bound set.

                // Note; we need to special-case the Table argument here.

                var bindingArg = boundArgs[argIdx];

                var fullTextValue = bindingArg.GetFullText(executionArguments.Scope, executionArguments.Step.RawText, executionArguments.Variables);


            }

            return bindResult;
        }

        /// <summary>
        /// Implementations should return a value indicating whether the provided step definition is the same as this one (but possibly a different version).
        /// </summary>
        /// <param name="def">The definition to check against.</param>
        /// <returns>True if the definition is semantically the same.</returns>
        public override bool IsSameDefinition(StepDefinition def)
        {
            // It's the same definition if it's the same method handle.
            if (def is BuiltStepDefinition builtDef)
            {
                return builtDef.method.MethodHandle == method.MethodHandle;
            }

            return false;
        }
    }
}
