using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using AutoStep.Elements.Metadata;
using AutoStep.Execution.Contexts;
using AutoStep.Execution.Control;
using AutoStep.Execution.Dependency;
using AutoStep.Execution.Events;
using AutoStep.Execution.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace AutoStep.Execution.Strategy
{
    /// <summary>
    /// Implements the default scenario execution strategy.
    /// </summary>
    internal class DefaultScenarioExecutionStrategy : IScenarioExecutionStrategy
    {
        /// <summary>
        /// Execute the strategy.
        /// </summary>
        /// <param name="featureScope">The current service scope (which will be a feature scope).</param>
        /// <param name="featureContext">The current feature context.</param>
        /// <param name="scenario">The scenario metadata.</param>
        /// <param name="variableSet">The set of variables currently in-scope.</param>
        /// <param name="cancelToken">Cancellation token for the scenario.</param>
        /// <returns>A task that should complete when the scenario has finished executing.</returns>
        public async ValueTask ExecuteAsync(ILifetimeScope featureScope, FeatureContext featureContext, IScenarioInfo scenario, VariableSet variableSet, CancellationToken cancelToken)
        {
            var scenarioContext = new ScenarioContext(scenario, variableSet);

            using var scenarioScope = featureScope.BeginContextScope(ScopeTags.ScenarioTag, scenarioContext);

            var collectionExecutor = scenarioScope.Resolve<IStepCollectionExecutionStrategy>();
            var executionManager = scenarioScope.Resolve<IExecutionStateManager>();
            var events = scenarioScope.Resolve<IEventPipeline>();
            var contextProvider = scenarioScope.Resolve<IContextScopeProvider>();

            using (contextProvider.EnterContextScope(scenarioContext))
            {
                // Halt before the scenario begins.
                var haltInstruction = await executionManager.CheckforHalt(scenarioScope, scenarioContext, TestThreadState.StartingScenario).ConfigureAwait(false);

                try
                {
                    await events.InvokeEventAsync(
                        scenarioScope,
                        scenarioContext,
                        (handler, sc, ctxt, next, cancel) => handler.OnScenarioAsync(sc, ctxt, next, cancel),
                        cancelToken,
                        async (_, ctxt, cancel) =>
                    {
                        scenarioContext.ScenarioRan = true;

                        var scenarioTimer = new Stopwatch();
                        scenarioTimer.Start();

                        try
                        {
                            if (featureContext.Feature.Background is object)
                            {
                                // There is a background to execute.
                                await collectionExecutor.ExecuteAsync(
                                            scenarioScope,
                                            ctxt,
                                            featureContext.Feature.Background,
                                            variableSet,
                                            cancelToken).ConfigureAwait(false);

                                if (ctxt.FailException is object)
                                {
                                    // Something went wrong executing the background. Don't run the main scenario body.
                                    return;
                                }
                            }

                            // Any errors will be updated on the scenario context.
                            await collectionExecutor.ExecuteAsync(
                                    scenarioScope,
                                    scenarioContext,
                                    scenario,
                                    variableSet,
                                    cancelToken).ConfigureAwait(false);
                        }
                        finally
                        {
                            scenarioTimer.Stop();
                            scenarioContext.Elapsed = scenarioTimer.Elapsed;
                        }
                    }).ConfigureAwait(false);
                }
                catch (OperationCanceledException ex)
                {
                    scenarioContext.FailException = ex;
                }
                catch (EventHandlingException ex)
                {
                    // Something went wrong in the event handler, fail the scenario.
                    scenarioContext.FailException = ex;
                }
            }
        }
    }
}
