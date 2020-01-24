using System;
using AutoStep.Execution.Contexts;

namespace AutoStep.Execution.Dependency
{
    /// <summary>
    /// Providess access to services in the current scope.
    /// </summary>
    public interface IServiceScope : IDisposable
    {
        /// <summary>
        /// Gets the tag of the current scope (a value from <see cref="ScopeTags"/>).
        /// </summary>
        string Tag { get; }

        /// <summary>
        /// Resolves a typed service.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <returns>An instance of the type.</returns>
        /// <exception cref="DependencyException">Thrown if the service cannot be resolved.</exception>
        TService Resolve<TService>();

        /// <summary>
        /// Resolves a typed service.
        /// </summary>
        /// <typeparam name="TService">The declared service type.</typeparam>
        /// <param name="serviceType">The service.</param>
        /// <returns>An instance of the type.</returns>
        /// <exception cref="DependencyException">Thrown if the service cannot be resolved.</exception>
        TService Resolve<TService>(Type serviceType);

        /// <summary>
        /// Resolves a typed service.
        /// </summary>
        /// <param name="serviceType">The service.</param>
        /// <returns>An instance of the type.</returns>
        /// <exception cref="DependencyException">Thrown if the service cannot be resolved.</exception>
        object Resolve(Type serviceType);

        /// <summary>
        /// Begins a new scope, providing the associated context object to register within the scope.
        /// </summary>
        /// <typeparam name="TContext">The context type.</typeparam>
        /// <param name="contextInstance">The context object.</param>
        /// <returns>A new scope.</returns>
        /// <remarks>Make sure you dispose of the resulting scope.</remarks>
        IServiceScope BeginNewScope<TContext>(TContext contextInstance)
            where TContext : TestExecutionContext;

        /// <summary>
        /// Begins a new tagged scope, providing the associated context object to register within the scope.
        /// </summary>
        /// <typeparam name="TContext">The context type.</typeparam>
        /// <param name="scopeTag">The tag (usually a value from <see cref="ScopeTags"/>).</param>
        /// <param name="contextInstance">The context object.</param>
        /// <returns>A new scope.</returns>
        /// <remarks>Make sure you dispose of the resulting scope.</remarks>
        IServiceScope BeginNewScope<TContext>(string scopeTag, TContext contextInstance)
            where TContext : TestExecutionContext;
    }
}
