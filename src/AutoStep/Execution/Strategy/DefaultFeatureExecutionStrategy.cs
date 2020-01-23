using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AutoStep.Elements;
using AutoStep.Elements.ReadOnly;
using AutoStep.Execution.Contexts;
using AutoStep.Execution.Control;
using AutoStep.Execution.Dependency;
using AutoStep.Execution.Events;

namespace AutoStep.Execution.Strategy
{
    public class DefaultFeatureExecutionStrategy : IFeatureExecutionStrategy
    {
        public async ValueTask Execute(IServiceScope threadScope, IEventPipeline events, IFeatureInfo feature)
        {
            var featureContext = new FeatureContext(feature);

            using var featureScope = threadScope.BeginNewScope(ScopeTags.FeatureTag, featureContext);

            var executionManager = featureScope.Resolve<IExecutionStateManager>();
            var scenarioStrategy = featureScope.Resolve<IScenarioExecutionStrategy>();

            await events.InvokeEvent(featureScope, featureContext, (handler, sc, ctxt, next) => handler.Feature(sc, ctxt, next), async (featureScope, featureContext) =>
            {
                var haltInstruction = await executionManager.CheckforHalt(featureScope, featureContext, TestThreadState.StartingFeature).ConfigureAwait(false);

                // Go through each scenario.
                foreach (var scenario in feature.Scenarios)
                {
                    foreach (var variableSet in ExpandScenario(scenario, featureScope))
                    {
                        await scenarioStrategy.Execute(
                            featureScope,
                            featureContext,
                            scenario,
                            variableSet).ConfigureAwait(false);
                    }
                }

            }).ConfigureAwait(false);
        }

        private IEnumerable<VariableSet> ExpandScenario(IScenarioInfo scenario, IServiceScope scope)
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
