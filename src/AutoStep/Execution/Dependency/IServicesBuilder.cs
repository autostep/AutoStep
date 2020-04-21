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
        void RegisterPerResolveService<TService>()
            where TService : class;

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
        void RegisterPerFeatureService<TService, TComponent>()
            where TService : class
            where TComponent : TService;

        /// <summary>
        /// Register a service that has one instance for each feature.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        void RegisterPerFeatureService<TService>()
            where TService : class;

        /// <summary>
        /// Register a service that has one instance for each scenario.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <typeparam name="TComponent">The implementing type.</typeparam>
        void RegisterPerScenarioService<TService, TComponent>()
            where TService : class
            where TComponent : TService;

        /// <summary>
        /// Register a service that has one instance for each scenario.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        void RegisterPerScenarioService<TService>()
            where TService : class;

        /// <summary>
        /// Register a service that has one instance for each step.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <typeparam name="TComponent">The implementing type.</typeparam>
        void RegisterPerStepService<TService, TComponent>()
            where TService : class
            where TComponent : TService;

        /// <summary>
        /// Register a service that has one instance for each step.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        void RegisterPerStepService<TService>()
            where TService : class;

        /// <summary>
        /// Register a service that has one instance for each test execution thread.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        void RegisterPerThreadService<TService>()
            where TService : class;

        /// <summary>
        /// Register a service that has one instance for each test execution thread.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <typeparam name="TComponent">The implementing type.</typeparam>
        void RegisterPerThreadService<TService, TComponent>()
            where TService : class
            where TComponent : TService;

        /// <summary>
        /// Register a singleton instance (the same object will always be returned, whenever it is resolved).
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <param name="instance">The instance to register.</param>
        /// <remarks>
        /// Take care with thread-safety on all services registered as a singleton; all test threads
        /// will use the same object.
        /// </remarks>
        void RegisterInstance<TService>(TService instance)
            where TService : class;

        /// <summary>
        /// Register a singleton instance (the same object will always be returned, whenever it is resolved).
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <remarks>
        /// Take care with thread-safety on all services registered as a singleton; all test threads
        /// will use the same object.
        /// </remarks>
        void RegisterSingleton<TService>()
            where TService : class;

        /// <summary>
        /// Register a singleton instance (the same object will always be returned, whenever it is resolved).
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <typeparam name="TComponent">The component type.</typeparam>
        /// <remarks>
        /// Take care with thread-safety on all services registered as a singleton; all test threads
        /// will use the same object.
        /// </remarks>
        void RegisterSingleton<TService, TComponent>()
            where TService : class
            where TComponent : TService;

        /// <summary>
        /// Create a root scope from this builder.
        /// </summary>
        /// <returns>The root of the scope hierarchy.</returns>
        IAutoStepServiceScope BuildRootScope();
    }
}
