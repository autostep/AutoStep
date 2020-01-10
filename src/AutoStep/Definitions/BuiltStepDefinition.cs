using System;
using System.Reflection;

namespace AutoStep.Definitions
{
    /// <summary>
    /// Represents a step definition backed by a method in code.
    /// </summary>
    public class BuiltStepDefinition : StepDefinition
    {
        private readonly MethodInfo method;

        /// <summary>
        /// Initializes a new instance of the <see cref="BuiltStepDefinition"/> class.
        /// </summary>
        /// <param name="source">The source used to load the step definition.</param>
        /// <param name="method">The method info representing the method to bind against.</param>
        /// <param name="declaringAttribute">The attribute that declared the step definition.</param>
        public BuiltStepDefinition(AssemblyStepDefinitionSource source, MethodInfo method, StepDefinitionAttribute declaringAttribute)
            : base(source, declaringAttribute?.Type ?? throw new ArgumentNullException(nameof(declaringAttribute)), declaringAttribute.Declaration)
        {
            this.method = method;
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
