using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using AutoStep.Definitions.Interaction;
using AutoStep.Execution.Binding;
using AutoStep.Execution.Contexts;
using AutoStep.Execution.Control;
using AutoStep.Execution.Dependency;
using AutoStep.Execution.Events;
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
        /// <param name="serviceRegistration">An optional callback that allows additional services to be registered.</param>
        /// <returns>A task that completes when the run completes, including the final run context.</returns>
        public async Task<RunContext> ExecuteAsync(CancellationToken cancelToken, Action<IConfiguration, IServicesBuilder>? serviceRegistration = null)
        {
            using var nullLogger = new LoggerFactory();

            return await ExecuteAsync(nullLogger, cancelToken, serviceRegistration).ConfigureAwait(false);
        }

        /// <summary>
        /// Execute a test run.
        /// </summary>
        /// <param name="logFactory">A logger factory.</param>
        /// <param name="cancelToken">A cancellation token for the test run.</param>
        /// <param name="serviceRegistration">An optional callback that allows additional services to be registered.</param>
        /// <returns>A task that completes when the run completes, including the final run context.</returns>
        public async Task<RunContext> ExecuteAsync(ILoggerFactory logFactory, CancellationToken cancelToken, Action<IConfiguration, IServicesBuilder>? serviceRegistration = null)
        {
            // Determined the filtered set of features/scenarios.
            var executionSet = FeatureExecutionSet.Create(Project, filter, logFactory);

            var logger = logFactory.CreateLogger<TestRun>();

            var builtConfiguration = ConfigurationBuilder.Build();

            // Create a top-level run context
            var runContext = new RunContext(builtConfiguration);

            if (executionSet.Features.Count == 0)
            {
                // No features. What should we do? Just return an empty run result, no point continuing really.
                logger.LogInformation(ExecutionText.TestRun_NoFeatures);
                return runContext;
            }

            // Build the pipeline.
            var events = eventPipelineBuilder.Build();

            // Build the container and prepare a root scope.
            using var rootScope = PrepareContainer(events, logFactory, builtConfiguration, serviceRegistration, executionSet);

            // Run scope (disposes at the end of the method).
            using var runScope = rootScope.BeginNewScope(ScopeTags.RunTag, runContext);

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

            return runContext;
        }

        private IAutoStepServiceScope PrepareContainer(EventPipeline events, ILoggerFactory logFactory, IConfigurationRoot builtConfiguration, Action<IConfiguration, IServicesBuilder>? serviceRegistration, FeatureExecutionSet featureSet)
        {
            // Built the DI container for the execution.
            var exposedServiceRegistration = new AutofacServiceBuilder();

            exposedServiceRegistration.RegisterInstance(logFactory);

            // Register our strategies.
            exposedServiceRegistration.RegisterInstance(runExecutionStrategy);
            exposedServiceRegistration.RegisterInstance(featureExecutionStrategy);
            exposedServiceRegistration.RegisterInstance(stepCollectionExecutionStrategy);
            exposedServiceRegistration.RegisterInstance(stepExecutionStrategy);
            exposedServiceRegistration.RegisterInstance(scenarioStrategy);

            // Register our argument binder registry.
            var argumentBinderRegistry = new ArgumentBinderRegistry();
            exposedServiceRegistration.RegisterInstance(argumentBinderRegistry);

            // Register the execution manager.
            exposedServiceRegistration.RegisterInstance(executionManager);

            // Add our built event pipeline to DI.
            exposedServiceRegistration.RegisterInstance<IEventPipeline>(events);

            // Register the entire set in the container.
            exposedServiceRegistration.RegisterInstance(featureSet);

            // Register configuration concepts in the container.
            exposedServiceRegistration.RegisterInstance(builtConfiguration);

            ConfigureLanguageServices(exposedServiceRegistration, Project.Compiler, builtConfiguration);

            serviceRegistration?.Invoke(builtConfiguration, exposedServiceRegistration);

            return exposedServiceRegistration.BuildRootScope();
        }

        private static void ConfigureLanguageServices(IServicesBuilder exposedServiceRegistration, IProjectCompiler compiler, IConfiguration configuration)
        {
            // Ask the project's compiler for the list of step definition sources.
            foreach (var source in compiler.EnumerateStepDefinitionSources())
            {
                // Let each step definition source register services (e.g. step classes).
                source.ConfigureServices(exposedServiceRegistration, configuration);
            }

            // Iterate over the methods in the root method table.
            foreach (var service in compiler.Interactions.RootMethodTable.GetAllMethodProvidingServices())
            {
                exposedServiceRegistration.RegisterPerResolveService(service);
            }
        }
    }
}
