using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using AutoStep.Elements.Metadata;
using AutoStep.Execution.Contexts;
using AutoStep.Execution.Control;
using AutoStep.Execution.Dependency;
using AutoStep.Execution.Events;
using AutoStep.Execution.Logging;

namespace AutoStep.Execution.Strategy
{
    /// <summary>
    /// Implements the default feature execution strategy.
    /// </summary>
    public class DefaultFeatureExecutionStrategy : IFeatureExecutionStrategy
    {
        /// <summary>
        /// Execute the strategy.
        /// </summary>
        /// <param name="threadScope">The current service scope (which will be a thread scope).</param>
        /// <param name="threadContext">The test thread context.</param>
        /// <param name="feature">The feature metadata.</param>
        /// <param name="cancelToken">Cancellation token for the feature.</param>
        /// <returns>A task that should complete when the feature has finished executing.</returns>
        public async ValueTask ExecuteAsync(ILifetimeScope threadScope, ThreadContext threadContext, IFeatureInfo feature, CancellationToken cancelToken)
        {
            threadScope = threadScope.ThrowIfNull(nameof(threadScope));

            var featureContext = new FeatureContext(feature);

            using var featureScope = threadScope.BeginContextScope(ScopeTags.FeatureTag, featureContext);

            var executionManager = featureScope.Resolve<IExecutionStateManager>();
            var scenarioStrategy = featureScope.Resolve<IScenarioExecutionStrategy>();
            var events = featureScope.Resolve<IEventPipeline>();
            var contextProvider = threadScope.Resolve<IContextScopeProvider>();

            using (contextProvider.EnterContextScope(featureContext))
            {
                try
                {
                    await events.InvokeEventAsync(
                        featureScope,
                        featureContext,
                        (handler, sc, ctxt, next, cancel) => handler.OnFeatureAsync(sc, ctxt, next, cancel),
                        cancelToken,
                        async (_, featureContext, cancel) =>
                    {
                        var haltInstruction = await executionManager.CheckforHalt(featureScope, featureContext, TestThreadState.StartingFeature).ConfigureAwait(false);

                        featureContext.FeatureRan = true;

                        // Go through each scenario.
                        foreach (var scenario in feature.Scenarios)
                            {
                                if (cancelToken.IsCancellationRequested)
                                {
                                    break;
                                }

                                foreach (var variableSet in ExpandScenario(scenario, featureScope))
                                {
                                    await scenarioStrategy.ExecuteAsync(
                                        featureScope,
                                        featureContext,
                                        scenario,
                                        variableSet,
                                        cancel).ConfigureAwait(false);
                                }
                            }
                        }).ConfigureAwait(false);
                }
                catch (OperationCanceledException ex)
                {
                    featureContext.FeatureFailureException = ex;
                }
                catch (EventHandlingException ex)
                {
                    // Event handler fail; store the exception against the feature context.
                    featureContext.FeatureFailureException = ex;
                }
            }
        }

        private IEnumerable<VariableSet> ExpandScenario(IScenarioInfo scenario, ILifetimeScope scope)
        {
            if (scenario is IScenarioOutlineInfo outline)
            {
                // One scenario for each example. For now we will flatten the scenario outline into different scenarios.
                foreach (var example in outline.Examples)
                {
                    if (example.Table is null)
                    {
                        // Can't be here, unless the parser is broken.
                        throw new LanguageEngineAssertException();
                    }

                    foreach (var variableSet in TableVariableSet.CreateSetsForRows(example.Table, scope, VariableSet.Blank))
                    {
                        yield return variableSet;
                    }
                }
            }
            else
            {
                yield return VariableSet.Blank;
            }
        }
    }
}
