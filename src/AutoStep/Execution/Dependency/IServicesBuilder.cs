using System;
using AutoStep.Execution.Events;

namespace AutoStep.Execution.Dependency
{
    /// <summary>
    /// Defines the interface for a DI services builder.
    /// </summary>
    public interface IServicesBuilder
    {
        /// <summary>
        /// Register a type that is resolved fresh each time it is requested.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        void RegisterPerResolveService<TService>();

        /// <summary>
        /// Register a type that is resolved fresh each time it is requested.
        /// </summary>
        /// <param name="consumer">The service type.</param>
        void RegisterPerResolveService(Type consumer);

        /// <summary>
        /// Register a service that has one instance for each feature.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <typeparam name="TComponent">The implementing type.</typeparam>
        void RegisterPerFeatureService<TService, TComponent>();

        /// <summary>
        /// Register a service that has one instance for each feature.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        void RegisterPerFeatureService<TService>();

        /// <summary>
        /// Register a service that has one instance for each scenario.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <typeparam name="TComponent">The implementing type.</typeparam>
        void RegisterPerScenarioService<TService, TComponent>();

        /// <summary>
        /// Register a service that has one instance for each scenario.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        void RegisterPerScenarioService<TService>();

        /// <summary>
        /// Register a service that has one instance for each scope (typically this is per-step).
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <typeparam name="TComponent">The implementing type.</typeparam>
        void RegisterPerScopeService<TService, TComponent>();

        /// <summary>
        /// Register a service that has one instance for each scope (typically this is per-step).
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        void RegisterPerScopeService<TService>();

        /// <summary>
        /// Register a service that has one instance for each test execution thread.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        void RegisterPerThreadService<TService>();

        /// <summary>
        /// Register a service that has one instance for each test execution thread.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <typeparam name="TComponent">The implementing type.</typeparam>
        void RegisterPerThreadService<TService, TComponent>();

        /// <summary>
        /// Register a singleton instance (the same object will always be returned, whenever it is resolved.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <param name="instance">The instance to register.</param>
        /// <remarks>
        /// Take care with thread-safety on all services registered as a singleton; all test threads
        /// will use the same object.
        /// </remarks>
        void RegisterSingleInstance<TService>(TService instance)
            where TService : class;

        /// <summary>
        /// Register an event handler.
        /// </summary>
        /// <param name="eventHandler">The event handler to register.</param>
        void RegisterEventHandler(IEventHandler eventHandler);

        /// <summary>
        /// Create a root scope from this builder.
        /// </summary>
        /// <returns>The root of the scope hierarchy.</returns>
        IServiceScope BuildRootScope();
    }
}
