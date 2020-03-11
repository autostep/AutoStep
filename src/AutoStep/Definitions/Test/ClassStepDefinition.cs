using System;
using System.Reflection;
using AutoStep.Execution.Dependency;

namespace AutoStep.Definitions.Test
{
    /// <summary>
    /// Represents a step definition backed by a method in code.
    /// </summary>
    public class ClassStepDefinition : MethodBackedStepDefinition
    {
        private readonly Type owner;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClassStepDefinition"/> class.
        /// </summary>
        /// <param name="source">The source used to load the step definition.</param>
        /// <param name="owner">The owning type.</param>
        /// <param name="method">The method info representing the method to bind against.</param>
        /// <param name="declaringAttribute">The attribute that declared the step definition.</param>
        public ClassStepDefinition(IStepDefinitionSource source, Type owner, MethodInfo method, StepDefinitionAttribute declaringAttribute)
            : base(source, method, declaringAttribute?.Type ?? throw new ArgumentNullException(nameof(declaringAttribute)), declaringAttribute.Declaration)
        {
            this.owner = owner;
        }

        /// <summary>
        /// Invokes the step method. Resolves an instance of the owning type and then executes the method.
        /// </summary>
        /// <param name="scope">The current scope to resolve from.</param>
        /// <returns>An instance of the class.</returns>
        protected override object GetMethodTarget(IServiceScope scope)
        {
            scope = scope.ThrowIfNull(nameof(scope));

            // Resolve an instance of the service. It will let it access any services.
            return scope.Resolve<object>(owner);
        }

        /// <summary>
        /// Implementations should return a value indicating whether the provided step definition is the same as this one (but possibly a different version).
        /// </summary>
        /// <param name="def">The definition to check against.</param>
        /// <returns>True if the definition is semantically the same.</returns>
        public override bool IsSameDefinition(StepDefinition def)
        {
            // It's the same definition if it's the same method handle.
            if (def is ClassStepDefinition builtDef)
            {
                return builtDef.Method.MethodHandle == Method.MethodHandle;
            }

            return false;
        }
    }
}
