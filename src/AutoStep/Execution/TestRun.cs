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
using AutoStep.Execution.Control;
using AutoStep.Execution.Dependency;
using AutoStep.Execution.Strategy;
using AutoStep.Projects;
using AutoStep.Tracing;

namespace AutoStep.Execution
{

    /// <summary>
    /// A class that represents the configuration used for a run. This will be the 'final' state, after all other configuration has been calculated,
    /// merged, etc.
    /// </summary>
    public class RunConfiguration
    {

    }

    /// <summary>
    /// Represents the outcome of a run. This should include run failure details, including aggregated feature/scenario results, as well as the 
    /// breakdown.
    /// </summary>
    public class RunResult
    {

    }
    
    public class TestRun
    {
        private readonly ProjectCompiler compiler;
        private readonly RunConfiguration configuration;
        private readonly IRunFilter filter;
        private readonly ITracer tracer;
        private readonly IExecutionStateManager executionManager;
        private readonly IScenarioExecutionStrategy scenarioStrategy;

        public Project Project { get; }

        public TestRun(
            Project project,
            ProjectCompiler compiler,
            RunConfiguration configuration,
            IRunFilter? filter = null,
            ITracer? tracer = null,
            IExecutionStateManager? executionStateManager = null)
        {
            Project = project;
            this.compiler = compiler;
            this.configuration = configuration;
            this.filter = filter ?? new RunAllFilter();
            this.tracer = tracer ?? NullTracer.Instance;
            this.scenarioStrategy = new DefaultScenarioExecutionStrategy(tracer);
            this.executionManager = executionStateManager ?? new DefaultExecutionStateManager();
        }

        internal TestRun(
          Project project,
          ProjectCompiler compiler,
          RunConfiguration configuration,
          IScenarioExecutionStrategy executionStrategy,
          IRunFilter? filter = null,
          ITracer? tracer = null,
          IExecutionStateManager? executionStateManager = null)
        {
            Project = project;
            this.compiler = compiler;
            this.configuration = configuration;
            this.filter = filter ?? new RunAllFilter();
            this.tracer = tracer ?? NullTracer.Instance;
            this.scenarioStrategy = executionStrategy;
            this.executionManager = executionStateManager ?? new DefaultExecutionStateManager();
        }

        public async Task<RunResult> Execute(Action<IEventPipelineBuilder>? eventBuilder = null, CancellationToken cancelToken = default)
        { 
            // TODO: Logging!

            // We'll run a compile and link to start; this ensures we are all up to date
            // (note that if everything is up to date, then nothing actually happens).
            await compiler.Compile(cancelToken).ConfigureAwait(false);

            // Link regardless.
            compiler.Link(cancelToken);

            // Built the DI container for the execution.
            // Go through all the step definitions and allow them to hook into services.
            var serviceBuilder = new ContainerBuilder();

            // TODO: Go through our modules and allow them to register services.

            var exposedServiceRegistration = new AutofacServiceBuilder(serviceBuilder);

            var pipelineBuilder = new EventPipelineBuilder();

            if (eventBuilder is object)
            {
                eventBuilder(pipelineBuilder);
            }

            // Let the event handlers configure services.
            var events = pipelineBuilder.Build();

            events.ConfigureServices(exposedServiceRegistration, configuration);

            // Determined the filtered set of features/scenarios.
            var executionSet = FeatureExecutionSet.Create(Project, filter, tracer);

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

            // Create a top-level run context (disposes at the end of the method).
            var runContext = new RunContext();

            using var runScope = rootScope.BeginNewScope(ScopeTags.RunTag, runContext);

            await events.InvokeEvent(
                runScope, 
                runContext, 
                (handler, sc, ctxt, next) => handler.Execute(sc, ctxt, next), 
                async (scope, ctxt) =>
                {
                    // Event handlers have all executed now.

                    // Create a queue of all features.
                    var featureQueue = new ConcurrentQueue<FeatureElement>(executionSet.Features);

                    // Will need to come from config.
                    var parallelConfig = 1;

                    var parallelValue = Math.Min(featureQueue.Count, parallelConfig);

                    // Create x tasks based on level of parallelism.
                    var parallelTasks = new Task[parallelValue];

                    FeatureElement? FeatureDeQueue(ConcurrentQueue<FeatureElement> queue)
                    {
                        if (queue.TryDequeue(out var result))
                        {
                            return result;
                        }

                        return null;
                    }

                    for (int idx = 0; idx < parallelValue; idx++)
                    {
                        // Initially we'll just go for a feature parallel, but eventually we will
                        // probably add support for a scenario parallel.
                        parallelTasks[idx] = Task.Run(() => TestThreadFeatureParallel(idx, () => FeatureDeQueue(featureQueue), events, runScope));
                    }

                    // Wait for test threads to finish.
                    await Task.WhenAll(parallelTasks).ConfigureAwait(false);
                }).ConfigureAwait(false);

            return new RunResult();
        }

        private async Task TestThreadFeatureParallel(int testThreadId, Func<FeatureElement?> nextFeature, EventPipeline events, IServiceScope scope)
        {
            // Event handler is set by only caller, which has its own catch.
            Debug.Assert(events is object);

            var threadContext = new ThreadContext(testThreadId);

            using var threadScope = scope.BeginNewScope(ScopeTags.ThreadTag, threadContext);

            await events.InvokeEvent(
                threadScope,
                threadContext,
                (handler, sc, ctxt, next) => handler.Thread(sc, ctxt, next),
                async (scope, ctxt) =>
                {
                    var haltInstruction = await executionManager.CheckforHalt(scope, ctxt, TestThreadState.Starting).ConfigureAwait(false);

                    // TODO: Do something with halt instruction (terminate, for example?).
                    while (true)
                    {
                        var feature = nextFeature();

                        if (feature is object)
                        {
                            // We have a feature.
                            await TestFeature(scope, events, feature).ConfigureAwait(false);
                        }
                        else
                        {
                            tracer.Debug("Test Thread ID {0}; no more features to run.", testThreadId);
                            break;
                        }
                    }
                }).ConfigureAwait(false);
        }

        private async Task TestFeature(IServiceScope scope, EventPipeline events, FeatureElement feature)
        {
            var featureContext = new FeatureContext(feature);

            using var featureScope = scope.BeginNewScope(ScopeTags.FeatureTag, featureContext);

            await events.InvokeEvent(featureScope, featureContext, (handler, sc, ctxt, next) => handler.Feature(sc, ctxt, next), async (sc, ctxt) =>
            {
                var haltInstruction = await executionManager.CheckforHalt(sc, ctxt, TestThreadState.StartingFeature).ConfigureAwait(false);

                // Go through each scenario.
                foreach (var scenario in feature.Scenarios)
                {
                    foreach (var variableSet in ExpandScenario(scenario))
                    {
                        await scenarioStrategy.Execute(
                            featureScope,
                            featureContext,
                            scenario,
                            variableSet,
                            events,
                            executionManager).ConfigureAwait(false);
                    }
                }
            }).ConfigureAwait(false);
        }

        private IEnumerable<VariableSet> ExpandScenario(ScenarioElement scenario)
        {
            if (scenario is ScenarioOutlineElement outline)
            {
                // One scenario for each example. For now we will flatten the scenario outline into different scenarios.
                foreach (var example in outline.Examples)
                {
                    if (example.Table is null)
                    {
                        // Can't be here, unless the parser is broken.
                        throw new LanguageEngineAssertException();
                    }

                    foreach (var variableSet in VariableSet.CreateSet(example.Table))
                    {
                        yield return variableSet;
                    }
                }
            }
            else
            {
                yield return VariableSet.CreateBlank();
            }
        }
    }
}
