using System;
using System.Reflection;
using AutoStep.Execution.Dependency;
using Microsoft.Extensions.DependencyInjection;

namespace AutoStep.Definitions.Interaction
{
    /// <summary>
    /// Represents an interaction method inside a class.
    /// </summary>
    public class ClassBackedInteractionMethod : DefinedInteractionMethod
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClassBackedInteractionMethod"/> class.
        /// </summary>
        /// <param name="name">The method name.</param>
        /// <param name="classType">The containing type.</param>
        /// <param name="method">The method info.</param>
        public ClassBackedInteractionMethod(string name, Type classType, MethodInfo method)
            : base(name, method)
        {
            ServiceType = classType;
        }

        /// <summary>
        /// Gets the service type that backs the method.
        /// </summary>
        public Type ServiceType { get; }

        /// <inheritdoc/>
        protected override object? GetMethodTarget(IServiceProvider scope)
        {
            if (scope is null)
            {
                throw new ArgumentNullException(nameof(scope));
            }

            // Resolve an instance of the type from the scope.
            return scope.GetRequiredService(ServiceType);
        }
    }
}
