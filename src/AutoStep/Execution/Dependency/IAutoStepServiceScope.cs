using System;
using AutoStep.Execution.Contexts;
using Microsoft.Extensions.DependencyInjection;

namespace AutoStep.Execution.Dependency
{
    /// <summary>
    /// Provides access to services in the current scope.
    /// </summary>
    public interface IAutoStepServiceScope : IDisposable, IServiceProvider
    {
        /// <summary>
        /// Gets the tag of the current scope (a value from <see cref="ScopeTags"/>).
        /// </summary>
        string Tag { get; }

        /// <summary>
        /// Begins a new tagged scope, providing the associated context object to register within the scope.
        /// </summary>
        /// <typeparam name="TContext">The context type.</typeparam>
        /// <param name="scopeTag">The tag (usually a value from <see cref="ScopeTags"/>).</param>
        /// <param name="contextInstance">The context object.</param>
        /// <returns>A new scope.</returns>
        /// <remarks>Make sure you dispose of the resulting scope.</remarks>
        IAutoStepServiceScope BeginNewScope<TContext>(string scopeTag, TContext contextInstance)
            where TContext : TestExecutionContext;
    }
}
