using System.Collections.Generic;
using System.Threading.Tasks;
using AutoStep.Definitions;
using AutoStep.Elements;
using AutoStep.Tracing;

namespace AutoStep.Execution
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

        public async Task Execute(FeatureContext featureContext, ScenarioElement scenario, VariableSet variableSet, EventManager events, IExecutionStateManager executionManager)
        {
            using var scenarioContext = new ScenarioContext(featureContext, scenario, variableSet);

            // Halt before the scenario begins.
            var haltInstruction = await executionManager.CheckforHalt(scenarioContext, TestThreadState.StartingScenario).ConfigureAwait(false);

            await events.InvokeEvent(scenarioContext, (handler, ctxt, next) => handler.Scenario(ctxt, next), ctxt =>
            {
                // Any errors will be udated on the scenario context.
                return collectionExecutionStrategy.Execute(scenarioContext, scenario, variableSet, events, executionManager);
            }).ConfigureAwait(false);
        }
    }
}
