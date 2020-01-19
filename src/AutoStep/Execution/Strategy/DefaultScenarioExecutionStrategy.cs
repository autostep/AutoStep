using System.Collections.Generic;
using System.Threading.Tasks;
using AutoStep.Definitions;
using AutoStep.Elements;
using AutoStep.Execution.Control;
using AutoStep.Execution.Dependency;
using AutoStep.Tracing;

namespace AutoStep.Execution.Strategy
{
    internal class DefaultScenarioExecutionStrategy : IScenarioExecutionStrategy
    {
        private readonly ITracer tracer;
        private readonly IStepCollectionExecutionStrategy collectionExecutionStrategy;

        public DefaultScenarioExecutionStrategy(ITracer tracer, IStepCollectionExecutionStrategy? stepCollectionExecutionStrategy = null)
        {
            this.tracer = tracer;
            collectionExecutionStrategy = stepCollectionExecutionStrategy ?? new StepCollectionExecutionStrategy();
        }

        public async Task Execute(IServiceScope featureScope, FeatureContext featureContext, ScenarioElement scenario, VariableSet variableSet, EventPipeline events, IExecutionStateManager executionManager)
        {
            var scenarioContext = new ScenarioContext(scenario, variableSet);

            using var scenarioScope = featureScope.BeginNewScope(ScopeTags.ScenarioTag, scenarioContext);

            // Halt before the scenario begins.
            var haltInstruction = await executionManager.CheckforHalt(scenarioScope, scenarioContext, TestThreadState.StartingScenario).ConfigureAwait(false);

            await events.InvokeEvent(scenarioScope, scenarioContext, (handler, sc, ctxt, next) => handler.Scenario(sc, ctxt, next), async (scope, ctxt) =>
            {
                if (featureContext.Feature.Background is object)
                {
                    // There is a background to execute.
                    await collectionExecutionStrategy.Execute(
                        scope,
                        ctxt,
                        featureContext.Feature.Background,
                        variableSet,
                        events,
                        executionManager).ConfigureAwait(false);
                }

                // Any errors will be udated on the scenario context.
                await collectionExecutionStrategy.Execute(
                    scenarioScope,
                    scenarioContext,
                    scenario,
                    variableSet,
                    events,
                    executionManager).ConfigureAwait(false);

            }).ConfigureAwait(false);
        }
    }
}
