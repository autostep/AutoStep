using System;
using System.Reflection;
using AutoStep.Execution.Dependency;

namespace AutoStep.Definitions.Interaction
{
    /// <summary>
    /// Represents an interaction method inside a class.
    /// </summary>
    public class ClassBackedInteractionMethod : DefinedInteractionMethod
    {
        private readonly Type implType;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClassBackedInteractionMethod"/> class.
        /// </summary>
        /// <param name="name">The method name.</param>
        /// <param name="classType">The containin type.</param>
        /// <param name="method">The method info.</param>
        public ClassBackedInteractionMethod(string name, Type classType, MethodInfo method)
            : base(name, method)
        {
            implType = classType;
        }

        /// <inheritdoc/>
        protected override object? GetMethodTarget(IServiceScope scope)
        {
            if (scope is null)
            {
                throw new ArgumentNullException(nameof(scope));
            }

            // Resolve an instance of the type from the scope.
            return scope.Resolve(implType);
        }
    }
}
