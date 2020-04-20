using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoStep.Elements.Metadata;
using AutoStep.Execution.Contexts;
using AutoStep.Execution.Control;
using AutoStep.Execution.Dependency;
using AutoStep.Execution.Events;
using Microsoft.Extensions.DependencyInjection;

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
        public async ValueTask ExecuteAsync(IAutoStepServiceScope threadScope, ThreadContext threadContext, IFeatureInfo feature, CancellationToken cancelToken)
        {
            threadScope = threadScope.ThrowIfNull(nameof(threadScope));

            var featureContext = new FeatureContext(feature);

            using var featureScope = threadScope.BeginNewScope(ScopeTags.FeatureTag, featureContext);

            var executionManager = featureScope.GetRequiredService<IExecutionStateManager>();
            var scenarioStrategy = featureScope.GetRequiredService<IScenarioExecutionStrategy>();
            var events = featureScope.GetRequiredService<IEventPipeline>();

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

        private IEnumerable<VariableSet> ExpandScenario(IScenarioInfo scenario, IServiceProvider scope)
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

                    foreach (var variableSet in VariableSet.CreateSetsForRows(example.Table, scope, VariableSet.Blank))
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
