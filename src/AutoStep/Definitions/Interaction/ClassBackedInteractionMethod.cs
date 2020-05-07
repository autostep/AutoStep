using System;
using System.Reflection;
using System.Text;
using AutoStep.Execution.Dependency;
using Microsoft.Extensions.DependencyInjection;

namespace AutoStep.Definitions.Interaction
{
    /// <summary>
    /// Represents an interaction method inside a class.
    /// </summary>
    public class ClassBackedInteractionMethod : DefinedInteractionMethod
    {
        private readonly InteractionMethodAttribute methodAttribute;
        private string? cachedProcessedDocumentation;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClassBackedInteractionMethod"/> class.
        /// </summary>
        /// <param name="name">The method name.</param>
        /// <param name="classType">The containing type.</param>
        /// <param name="method">The method info.</param>
        /// <param name="methodAttribute">The method declaration attribute attached to the method.</param>
        public ClassBackedInteractionMethod(string name, Type classType, MethodInfo method, InteractionMethodAttribute methodAttribute)
            : base(name, method)
        {
            ServiceType = classType;
            this.methodAttribute = methodAttribute;
        }

        /// <summary>
        /// Gets the service type that backs the method.
        /// </summary>
        public Type ServiceType { get; }

        /// <inheritdoc/>
        public override string? GetDocumentation()
        {
            if (cachedProcessedDocumentation is object)
            {
                return cachedProcessedDocumentation;
            }

            if (methodAttribute.Documentation is null)
            {
                return null;
            }

            return cachedProcessedDocumentation = DocumentationHelper.GetProcessedDocumentationBlock(methodAttribute.Documentation);
        }

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
