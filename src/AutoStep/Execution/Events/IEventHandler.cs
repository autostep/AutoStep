using System;
using System.Threading.Tasks;
using AutoStep.Execution.Contexts;
using AutoStep.Execution.Dependency;

namespace AutoStep.Execution.Events
{
    /// <summary>
    /// Defines an interface for an event handler.
    /// </summary>
    public interface IEventHandler
    {
        /// <summary>
        /// Called prior to execution, to allow the event handler to register any services it wants to be available during execution.
        /// </summary>
        /// <param name="builder">The services builder.</param>
        /// <param name="configuration">The test run configuration.</param>
        void ConfigureServices(IServicesBuilder builder, RunConfiguration configuration);

        /// <summary>
        /// Invoked at the Execution Stage. This occurs just after the set of testable scenarios have been determined,
        /// but before any threads are created. It is a good place to do global set-up.
        /// </summary>
        /// <param name="scope">The current DI scope. Only singleton services and per-scope services will be available.</param>
        /// <param name="ctxt">The run context for the test.</param>
        /// <param name="nextHandler">
        /// The next stage in the pipeline. Event handlers must call (and await) on this method if they want the test to continue.
        /// Once the <paramref name="nextHandler"/> has returned, the entire test run has finished.
        /// </param>
        /// <returns>A task that can be awaited on by the execution system or a prior event handler.</returns>
        ValueTask OnExecute(IServiceScope scope, RunContext ctxt, Func<IServiceScope, RunContext, ValueTask> nextHandler);

        /// <summary>
        /// Invoked at the Thread Stage. This occurs per-thread, just after the thread has been started, but before any features start executing.
        /// It is a good place to set up any services that need per-thread initialisation.
        /// </summary>
        /// <param name="scope">The current DI scope. Per-thread services will be available here.</param>
        /// <param name="ctxt">The context for the thread.</param>
        /// <param name="nextHandler">
        /// The next stage in the pipeline. Event handlers must call (and await) on this method if they want the test to continue.
        /// Once the <paramref name="nextHandler"/> has returned, the thread has finished execution (so clean-up can be performed).
        /// </param>
        /// <returns>A task that can be awaited on by the execution system or a prior event handler.</returns>
        ValueTask OnThread(IServiceScope scope, ThreadContext ctxt, Func<IServiceScope, ThreadContext, ValueTask> nextHandler);

        /// <summary>
        /// Invoked at the Feature Stage. This occurs per-thread and per-feature, just before the feature is started, but before any scenarios start executing.
        /// It is a good place to acquire resources for the feature.
        /// </summary>
        /// <param name="scope">The current DI scope. Per-feature services will be available here.</param>
        /// <param name="ctxt">The context for the feature (including any feature metadata).</param>
        /// <param name="nextHandler">
        /// The next stage in the pipeline. Event handlers must call (and await) on this method if they want the feature to be tested.
        /// Once the <paramref name="nextHandler"/> has returned, the feature has finished execution (so clean-up can be performed).
        /// </param>
        /// <returns>A task that can be awaited on by the execution system or a prior event handler.</returns>
        ValueTask OnFeature(IServiceScope scope, FeatureContext ctxt, Func<IServiceScope, FeatureContext, ValueTask> nextHandler);

        /// <summary>
        /// Invoked at the Scenario Stage. This occurs per-scenario, just before the scenario (and any background) is started.
        /// Each example set of a scenario outline will is treated as a new scenario, with different variables in <see cref="ScenarioContext.Variables"/>.
        /// </summary>
        /// <param name="scope">The current DI scope. Per-scenario services will be available here.</param>
        /// <param name="ctxt">The context for the scenario (including any scenario metadata).</param>
        /// <param name="nextHandler">
        /// The next stage in the pipeline. Event handlers must call (and await) on this method if they want the feature to be tested.
        /// Once the <paramref name="nextHandler"/> has returned, the scenario has finished execution (so clean-up can be performed).
        /// </param>
        /// <returns>A task that can be awaited on by the execution system or a prior event handler.</returns>
        ValueTask OnScenario(IServiceScope scope, ScenarioContext ctxt, Func<IServiceScope, ScenarioContext, ValueTask> nextHandler);

        /// <summary>
        /// Invoked at the Step Stage. This occurs per-step, just before the step executes.
        /// </summary>
        /// <param name="scope">The current DI scope.</param>
        /// <param name="ctxt">The context for the step (including any step metadata).</param>
        /// <param name="nextHandler">
        /// The next stage in the pipeline. Event handlers must call (and await) on this method if they want the step to run.
        /// Once the <paramref name="nextHandler"/> has returned, the feature has finished execution (so clean-up can be performed).
        /// </param>
        /// <returns>A task that can be awaited on by the execution system or a prior event handler.</returns>
        ValueTask OnStep(IServiceScope scope, StepContext ctxt, Func<IServiceScope, StepContext, ValueTask> nextHandler);
    }
}
