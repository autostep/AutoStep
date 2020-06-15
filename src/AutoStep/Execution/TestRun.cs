using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Autofac.Core.Lifetime;
using AutoStep.Definitions.Interaction;
using AutoStep.Execution.Binding;
using AutoStep.Execution.Contexts;
using AutoStep.Execution.Control;
using AutoStep.Execution.Dependency;
using AutoStep.Execution.Events;
using AutoStep.Execution.Logging;
using AutoStep.Execution.Strategy;
using AutoStep.Projects;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AutoStep.Execution
{
    /// <summary>
    /// Implements the top level 'test run'. Execution strategies can be changed here, and the test can be run with the Execute method.
    /// </summary>
    public class TestRun
    {
        private readonly IRunFilter filter;
        private readonly IExecutionStateManager executionManager;
        private readonly EventPipelineBuilder eventPipelineBuilder;
        private readonly List<Action<IConfiguration, ContainerBuilder>> cfgCallbacks = new List<Action<IConfiguration, ContainerBuilder>>();

        private IRunExecutionStrategy runExecutionStrategy;
        private IFeatureExecutionStrategy featureExecutionStrategy;
        private IScenarioExecutionStrategy scenarioStrategy;
        private IStepCollectionExecutionStrategy stepCollectionExecutionStrategy;
        private IStepExecutionStrategy stepExecutionStrategy;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestRun"/> class.
        /// </summary>
        /// <param name="project">The project to test.</param>
        /// <param name="projectConfiguration">The project configuration.</param>
        /// <param name="filter">A run filter, if there is one.</param>
        /// <param name="executionStateManager">An execution manager, if there is one.</param>
        public TestRun(
            Project project,
            IConfiguration? projectConfiguration = null,
            IRunFilter? filter = null,
            IExecutionStateManager? executionStateManager = null)
        {
            Project = project ?? throw new ArgumentNullException(nameof(project));

            ConfigurationBuilder = new ConfigurationBuilder();

            // Add the provided project configuration.
            if (projectConfiguration is object)
            {
                ConfigurationBuilder.AddConfiguration(projectConfiguration);
            }

            this.eventPipelineBuilder = new EventPipelineBuilder();
            this.filter = filter ?? new RunAllFilter();

            this.runExecutionStrategy = new DefaultRunExecutionStrategy();
            this.featureExecutionStrategy = new DefaultFeatureExecutionStrategy();
            this.scenarioStrategy = new DefaultScenarioExecutionStrategy();
            this.stepCollectionExecutionStrategy = new DefaultStepCollectionExecutionStrategy();
            this.stepExecutionStrategy = new DefaultStepExecutionStrategy();

            this.executionManager = executionStateManager ?? new DefaultExecutionStateManager();
        }

        /// <summary>
        /// Gets the project that is being tested.
        /// </summary>
        public Project Project { get; }

        /// <summary>
        /// Gets the event pipeline builder for the run.
        /// </summary>
        public IEventPipelineBuilder Events => eventPipelineBuilder;

        /// <summary>
        /// Gets the configuration being used for the test run.
        /// </summary>
        public IConfigurationBuilder ConfigurationBuilder { get; }

        /// <summary>
        /// Add a callback that will be invoked when the test run starts, to configure the set of available services.
        /// </summary>
        /// <param name="serviceSetupCallback">A callback to invoke that can configure the set of services.</param>
        public void AddServiceSetupCallback(Action<IConfiguration, ContainerBuilder> serviceSetupCallback)
        {
            if (serviceSetupCallback is null)
            {
                throw new ArgumentNullException(nameof(serviceSetupCallback));
            }

            // Will call this back later.
            cfgCallbacks.Add(serviceSetupCallback);
        }

        /// <summary>
        /// Change the Run Execution strategy for the test run.
        /// </summary>
        /// <param name="runStrategy">The new run strategy.</param>
        public void SetRunExecutionStrategy(IRunExecutionStrategy runStrategy)
        {
            runExecutionStrategy = runStrategy.ThrowIfNull(nameof(runStrategy));
        }

        /// <summary>
        /// Execute a test run.
        /// </summary>
        /// <param name="cancelToken">A cancellation token for the test run.</param>
        /// <param name="diConfiguration">An optional callback that allows additional services to be registered.</param>
        /// <returns>A task that completes when the run completes, including the final run context.</returns>
        public async Task<RunContext> ExecuteAsync(CancellationToken cancelToken, Action<IConfiguration, ContainerBuilder>? diConfiguration = null)
        {
            using var nullLogger = new LoggerFactory();

            return await ExecuteAsync(nullLogger, cancelToken, diConfiguration).ConfigureAwait(false);
        }

        /// <summary>
        /// Execute a test run.
        /// </summary>
        /// <param name="logFactory">A logger factory.</param>
        /// <param name="cancelToken">A cancellation token for the test run.</param>
        /// <param name="diConfiguration">An optional callback that allows additional services to be registered.</param>
        /// <returns>A task that completes when the run completes, including the final run context.</returns>
        public async Task<RunContext> ExecuteAsync(ILoggerFactory logFactory, CancellationToken cancelToken, Action<IConfiguration, ContainerBuilder>? diConfiguration = null)
        {
            var contextScopeProvider = new ContextScopeProvider();

            using var contextAwareLogFactory = new CapturingLogFactory(logFactory, contextScopeProvider);

            var builtConfiguration = ConfigurationBuilder.Build();

            // Create a top-level run context
            var runContext = new RunContext(builtConfiguration);

            using (contextScopeProvider.EnterContextScope(runContext))
            {
                var logger = contextAwareLogFactory.CreateLogger<TestRun>();

                // Determined the filtered set of features/scenarios.
                var executionSet = FeatureExecutionSet.Create(Project, filter, contextAwareLogFactory);

                if (executionSet.Features.Count == 0)
                {
                    // No features. What should we do? Just return an empty run result, no point continuing really.
                    logger.LogInformation(ExecutionText.TestRun_NoFeatures);
                    return runContext;
                }

                // Build the pipeline.
                var events = eventPipelineBuilder.Build();

                // Build the container and prepare a root scope.
                using var rootScope = PrepareContainer(events, contextAwareLogFactory, contextScopeProvider, builtConfiguration, diConfiguration, executionSet);

                // Run scope (disposes at the end of the method).
                using var runScope = rootScope.BeginContextScope(ScopeTags.RunTag, runContext);

                var timer = new Stopwatch();
                timer.Start();

                try
                {
                    await events.InvokeEventAsync(
                        runScope,
                        runContext,
                        (handler, sc, ctxt, next, cancel) => handler.OnExecuteAsync(sc, ctxt, next, cancel),
                        cancelToken,
                        (_, ctxt, cancel) =>
                        {
                            return new ValueTask(runExecutionStrategy.ExecuteAsync(runScope, ctxt, executionSet, cancel));
                        }).ConfigureAwait(false);
                }
                finally
                {
                    timer.Stop();
                    runContext.Duration = timer.Elapsed;
                }
            }

            return runContext;
        }

        private ILifetimeScope PrepareContainer(EventPipeline events, ILoggerFactory logFactory, ContextScopeProvider contextScopeProvider, IConfigurationRoot builtConfiguration, Action<IConfiguration, ContainerBuilder>? serviceRegistration, FeatureExecutionSet featureSet)
        {
            // Built the DI container for the execution.
            var containerBuilder = new ContainerBuilder();

            containerBuilder.RegisterInstance(logFactory);
            containerBuilder.RegisterGeneric(typeof(Logger<>)).As(typeof(ILogger<>));

            containerBuilder.RegisterInstance<IContextScopeProvider>(contextScopeProvider);

            // Register our strategies.
            containerBuilder.RegisterInstance(runExecutionStrategy);
            containerBuilder.RegisterInstance(featureExecutionStrategy);
            containerBuilder.RegisterInstance(stepCollectionExecutionStrategy);
            containerBuilder.RegisterInstance(stepExecutionStrategy);
            containerBuilder.RegisterInstance(scenarioStrategy);

            // Register our argument binder registry.
            var argumentBinderRegistry = new ArgumentBinderRegistry();
            containerBuilder.RegisterInstance(argumentBinderRegistry);

            // Register the execution manager.
            containerBuilder.RegisterInstance(executionManager);

            // Add our built event pipeline to DI.
            containerBuilder.RegisterInstance<IEventPipeline>(events);

            // Register the entire set in the container.
            containerBuilder.RegisterInstance(featureSet);

            // Register configuration concepts in the container.
            containerBuilder.RegisterInstance<IConfiguration>(builtConfiguration);

            ConfigureLanguageServices(containerBuilder, Project.Builder, builtConfiguration);

            foreach (var callback in cfgCallbacks)
            {
                callback(builtConfiguration, containerBuilder);
            }

            serviceRegistration?.Invoke(builtConfiguration, containerBuilder);

            return containerBuilder.Build();
        }

        private static void ConfigureLanguageServices(ContainerBuilder containerBuilder, IProjectBuilder builder, IConfiguration configuration)
        {
            // Ask the project's compiler for the list of step definition sources.
            foreach (var source in builder.EnumerateStepDefinitionSources())
            {
                // Let each step definition source register services (e.g. step classes).
                source.ConfigureServices(containerBuilder, configuration);
            }

            // Iterate over the methods in the root method table.
            foreach (var service in builder.Interactions.RootMethodTable.GetAllMethodProvidingServices())
            {
                containerBuilder.RegisterType(service).InstancePerDependency();
            }
        }
    }
}
