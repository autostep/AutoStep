using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using AutoStep.Elements;
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
    
    public interface IResultSource
    {
        void RegisterOnStartFeature();
        void RegisterOnEndFeature();

        void RegisterScenarioPassed();
        void RegisterScenarioFailed();

        void RegisterStepPassed();
        void RegisterStepFailed();

        event EventHandler StartFeature;
        event EventHandler EndFeature;

        event EventHandler ScenarioPassed;
        event EventHandler ScenarioFailed;

        event EventHandler StepPassed;
        event EventHandler StepFailed;
    }

    public class RunContext : ExecutionContext
    {
        internal RunContext(IServiceScope scope) : base(scope)
        {
        }
    }

    public class ThreadContext : ExecutionContext
    {
        public RunContext RunContext { get; }

        internal ThreadContext(int testThreadId, RunContext runCtxt)
            : base(runCtxt.Scope.BeginNewScope(ScopeTags.ThreadTag))
        {
            RunContext = runCtxt;
        }
    }

    public class ErrorCapturingContext : ExecutionContext
    {
        public ErrorCapturingContext(IServiceScope scope) : base(scope)
        {
        }

        public Exception? FailException { get; internal set; }

        public TimeSpan Elapsed { get; internal set; }
    }

    public class FeatureContext : ExecutionContext
    {
        internal FeatureContext(FeatureElement feature, ThreadContext threadContext)
            : base(threadContext.Scope.BeginNewScope(ScopeTags.FeatureTag))
        {
            Feature = feature;
        }

        public FeatureElement Feature { get; }
    }

    public class ScenarioContext : ErrorCapturingContext
    {
        private ExampleElement? example;

        internal ScenarioContext(FeatureContext featureContext, ScenarioElement scenario, VariableSet example)
            : base(featureContext.Scope.BeginNewScope(ScopeTags.ScenarioTag))
        {
            FeatureContext = featureContext;
            Scenario = scenario;
            Variables = example;
        }

        public FeatureContext FeatureContext { get; }

        public ScenarioElement Scenario { get; }

        public VariableSet Variables { get; }
    }

    public class StepContext : ErrorCapturingContext
    {
        internal StepContext(ExecutionContext parentContext, StepReferenceElement step, VariableSet variables)
            : base(parentContext.Scope.BeginNewScope(ScopeTags.StepTag))
        {
            ParentContext = parentContext;
            Step = step;
            Variables = variables;
        }

        public ExecutionContext ParentContext { get; }

        public StepReferenceElement Step { get; }

        public VariableSet Variables { get; }
    }

    public class TestRun
    {
        private readonly ProjectCompiler compiler;
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
            this.filter = filter ?? new RunAllFilter();
            this.tracer = tracer ?? NullTracer.Instance;
            this.scenarioStrategy = executionStrategy;
            this.executionManager = executionStateManager ?? new DefaultExecutionStateManager();
        }

        public async Task<RunResult> Execute(CancellationToken cancelToken = default)
        { 
            // TODO: Logging!

            // Prepare can only be called once. This method sets up any services for execution and loads plugins.

            // We'll run a compile and link to start; this ensures we are all up to date (note that if everything is up to date, then nothing actually happens).
            await compiler.Compile(cancelToken).ConfigureAwait(false);

            // Link regardless.
            compiler.Link(cancelToken);

            // Built the DI container for the execution.
            // Go through all the step definitions and allow them to hook into services.
            var serviceBuilder = new ContainerBuilder();

            // TODO: Go through our modules and allow them to register services.

            var exposedServiceRegistration = new AutofacServiceBuilder(serviceBuilder);

            foreach (var source in compiler.EnumerateStepDefinitionSources())
            {
                // Let each step definition source register services (e.g. step classes).
                source.RegisterExecutionServices(exposedServiceRegistration);
            }

            // Register the various context types.
            exposedServiceRegistration.RegisterPerThread<ThreadContext>();
            exposedServiceRegistration.RegisterPerFeatureService<FeatureContext>();
            exposedServiceRegistration.RegisterPerScenarioService<ScenarioContext>();
            exposedServiceRegistration.RegisterPerStepService<StepContext>();

            var container = serviceBuilder.Build();

            // Create our root scope.
            using var rootScope = new AutofacServiceScope(container);

            // TODO: Resolve any event handler implementations and remember them (to save us constantly resolving things).
            var events = new EventManager(rootScope.Resolve<IEnumerable<IEventHandler>>(), tracer);

            // Determined the filtered set of features/scenarios.
            var executionSet = FeatureExecutionSet.Create(Project, filter, tracer);

            // Create a top-level run context (disposes at the end of the method).
            using var runContext = new RunContext(rootScope.BeginNewScope(ScopeTags.RunTag));

            // Enter our entry/exit scope.
            await using (await events.EnterEventScope(runContext, (h, ctxt) => h.BeginExecute(ctxt), (h, ctxt) => h.EndExecute(ctxt)))
            {
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
                    parallelTasks[idx] = Task.Run(() => TestThreadFeatureParallel(idx, () => FeatureDeQueue(featureQueue), events, runContext));
                }

                // Wait for test threads to finish.
                await Task.WhenAll(parallelTasks).ConfigureAwait(false);

            }

            return new RunResult();
        }

        private async Task TestThreadFeatureParallel(int testThreadId, Func<FeatureElement?> nextFeature, EventManager events, RunContext runContext)
        {
            // Event handler is set by only caller, which has its own catch.
            Debug.Assert(events is object);

            using var threadContext = new ThreadContext(testThreadId, runContext);

            await using (await events.EnterEventScope(threadContext, (h, ctxt) => h.BeginThread(ctxt), (h, ctxt) => h.EndThread(ctxt)))
            {
                var haltInstruction = await executionManager.CheckforHalt(threadContext, TestThreadState.Starting).ConfigureAwait(false);

                // TODO: Do something with halt instruction (terminate, for example?).

                while (true)
                {
                    var feature = nextFeature();

                    if (feature is object)
                    {
                        // We have a feature.
                        await TestFeature(threadContext, events, feature).ConfigureAwait(false);
                    }
                    else
                    {
                        tracer.Debug("Test Thread ID {0}; no more features to run.", testThreadId);
                        break;
                    }
                }
            }
        }

        private async Task<IEnumerable<ScenarioResult>> TestFeature(ThreadContext threadContext, EventManager events, FeatureElement feature)
        {
            using var featureContext = new FeatureContext(feature, threadContext);

            await using (await events.EnterEventScope(featureContext, (h, ctxt) => h.BeginFeature(ctxt), (h, ctxt) => h.EndFeature(ctxt)))
            {
                var haltInstruction = await executionManager.CheckforHalt(threadContext, TestThreadState.StartingFeature).ConfigureAwait(false);

                // TODO: Handle feature halt; exit feature?
                var scenarioResults = new List<ScenarioResult>();

                // Go through each scenario.
                foreach (var scenario in feature.Scenarios)
                {
                    foreach (var variableSet in ExpandScenario(scenario))
                    {
                        var scenarioResult = await scenarioStrategy.Execute(
                            featureContext,
                            scenario,
                            variableSet,
                            events,
                            executionManager).ConfigureAwait(false);

                        scenarioResults.Add(scenarioResult);
                    }
                }

                return scenarioResults;
            }
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
