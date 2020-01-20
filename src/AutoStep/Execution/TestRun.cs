using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using AutoStep.Definitions;
using AutoStep.Elements;
using AutoStep.Execution.Binding;
using AutoStep.Execution.Control;
using AutoStep.Execution.Dependency;
using AutoStep.Execution.Strategy;
using AutoStep.Projects;
using AutoStep.Tracing;
using Microsoft.Extensions.Logging;

namespace AutoStep.Execution
{

    /// <summary>
    /// A class that represents the configuration used for a run. This will be the 'final' state, after all other configuration has been calculated,
    /// merged, etc.
    /// </summary>
    public class RunConfiguration
    {

    }

    public class TestRun
    {
        private readonly ProjectCompiler compiler;
        private readonly RunConfiguration configuration;
        private readonly ILoggerFactory logFactory;
        private readonly IRunFilter filter;
        private readonly IExecutionStateManager executionManager;

        private IRunExecutionStrategy runExecutionStrategy;
        private IFeatureExecutionStrategy featureExecutionStrategy;
        private IScenarioExecutionStrategy scenarioStrategy;
        private IStepCollectionExecutionStrategy stepCollectionExecutionStrategy;
        private IStepExecutionStrategy stepExecutionStrategy;

        public Project Project { get; }

        public TestRun(
            Project project,
            ProjectCompiler compiler,
            RunConfiguration configuration,
            ILoggerFactory logFactory,
            IRunFilter? filter = null,
            IExecutionStateManager? executionStateManager = null)
        {
            Project = project ?? throw new ArgumentNullException(nameof(project));
            this.compiler = compiler ?? throw new ArgumentNullException(nameof(compiler));
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this.logFactory = logFactory;
            this.filter = filter ?? new RunAllFilter();
            this.runExecutionStrategy = new DefaultRunExecutionStrategy();
            this.featureExecutionStrategy = new DefaultFeatureExecutionStrategy();
            this.scenarioStrategy = new DefaultScenarioExecutionStrategy();
            this.stepCollectionExecutionStrategy = new DefaultStepCollectionExecutionStrategy();
            this.stepExecutionStrategy = new DefaultStepExecutionStrategy();

            this.executionManager = executionStateManager ?? new DefaultExecutionStateManager();
        }

        public void SetRunExecutionStrategy(IRunExecutionStrategy runStrategy)
        {
            if (runStrategy is null)
            {
                throw new ArgumentNullException(nameof(runStrategy));
            }

            runExecutionStrategy = runStrategy;
        }

        public async Task<RunContext> Execute(Action<IEventPipelineBuilder>? eventBuilder = null, CancellationToken cancelToken = default)
        {

            // TODO: Logging!

            // We'll run a compile and link to start; this ensures we are all up to date
            // (note that if everything is up to date, then nothing actually happens).
            await compiler.Compile(cancelToken).ConfigureAwait(false);

            // Link regardless.
            compiler.Link(cancelToken);

            // Determined the filtered set of features/scenarios.
            var executionSet = FeatureExecutionSet.Create(Project, filter, logFactory);

            // Create a top-level run context
            var runContext = new RunContext();

            if (executionSet.Features.Count == 0)
            {
                // No features. What should we do? Just return an empty run result, no point continuing really.
                // Trace.
                return runContext;
            }

            // Built the DI container for the execution.
            // Go through all the step definitions and allow them to hook into services.
            var serviceBuilder = new ContainerBuilder();

            serviceBuilder.RegisterGeneric(typeof(LoggerWrapper<>)).As(typeof(ILogger<>));
            serviceBuilder.RegisterInstance(logFactory);

            var exposedServiceRegistration = new AutofacServiceBuilder(serviceBuilder);

            // Register our strategies.
            exposedServiceRegistration.RegisterSingleInstance(runExecutionStrategy);
            exposedServiceRegistration.RegisterSingleInstance(featureExecutionStrategy);
            exposedServiceRegistration.RegisterSingleInstance(stepCollectionExecutionStrategy);
            exposedServiceRegistration.RegisterSingleInstance(stepExecutionStrategy);
            exposedServiceRegistration.RegisterSingleInstance(scenarioStrategy);

            // Register our argument binder registry.
            var argumentBinderRegistry = new ArgumentBinderRegistry();
            exposedServiceRegistration.RegisterSingleInstance(argumentBinderRegistry);

            // Register the execution manager.
            exposedServiceRegistration.RegisterSingleInstance(executionManager);

            // TODO: Go through our modules and allow them to register services.

            var pipelineBuilder = new EventPipelineBuilder();

            if (eventBuilder is object)
            {
                eventBuilder(pipelineBuilder);
            }

            // Let the event handlers configure services.
            var events = pipelineBuilder.Build();

            // Add our built event pipeline to DI.
            exposedServiceRegistration.RegisterSingleInstance<IEventPipeline>(events);

            events.ConfigureServices(exposedServiceRegistration, configuration);

            // Register the entire set in the container.
            exposedServiceRegistration.RegisterSingleInstance(executionSet);

            foreach (var source in compiler.EnumerateStepDefinitionSources())
            {
                // Let each step definition source register services (e.g. step classes).
                source.ConfigureServices(exposedServiceRegistration, configuration);
            }

            var container = serviceBuilder.Build();

            // Create our root scope.
            using var rootScope = new AutofacServiceScope(ScopeTags.Root, container);

            // Run scope (disposes at the end of the method).
            using var runScope = rootScope.BeginNewScope(ScopeTags.RunTag, runContext);

            await events.InvokeEvent(
                runScope,
                runContext,
                (handler, sc, ctxt, next) => handler.Execute(sc, ctxt, next),
                (scope, ctxt) =>
                {
                    return runExecutionStrategy.Execute(scope, ctxt, executionSet, events);
                }).ConfigureAwait(false);

            return runContext;
        }

    }
}
