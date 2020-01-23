using System.Collections.Generic;
using System.Threading.Tasks;
using AutoStep.Definitions;
using AutoStep.Elements;
using AutoStep.Elements.ReadOnly;
using AutoStep.Execution.Contexts;
using AutoStep.Execution.Control;
using AutoStep.Execution.Dependency;
using AutoStep.Execution.Events;
using AutoStep.Tracing;

namespace AutoStep.Execution.Strategy
{
    internal class DefaultScenarioExecutionStrategy : IScenarioExecutionStrategy
    {
        public async ValueTask Execute(IServiceScope featureScope, FeatureContext featureContext, IScenarioInfo scenario, VariableSet variableSet)
        {
            var scenarioContext = new ScenarioContext(scenario, variableSet);

            var collectionExecutor = featureScope.Resolve<IStepCollectionExecutionStrategy>();
            var executionManager = featureScope.Resolve<IExecutionStateManager>();
            var events = featureScope.Resolve<IEventPipeline>();

            using var scenarioScope = featureScope.BeginNewScope(ScopeTags.ScenarioTag, scenarioContext);

            // Halt before the scenario begins.
            var haltInstruction = await executionManager.CheckforHalt(scenarioScope, scenarioContext, TestThreadState.StartingScenario).ConfigureAwait(false);

            try
            {
                await events.InvokeEvent(scenarioScope, scenarioContext, (handler, sc, ctxt, next) => handler.Scenario(sc, ctxt, next), async (scope, ctxt) =>
                {
                    if (featureContext.Feature.Background is object)
                    {
                        // There is a background to execute.
                        await collectionExecutor.Execute(
                                scope,
                                ctxt,
                                featureContext.Feature.Background,
                                variableSet).ConfigureAwait(false);

                        if (ctxt.FailException is object)
                        {
                            // Something went wrong executing the background. Don't run the main scenario body.
                            return;
                        }
                    }

                    // Any errors will be udated on the scenario context.
                    await collectionExecutor.Execute(
                            scenarioScope,
                            scenarioContext,
                            scenario,
                            variableSet).ConfigureAwait(false);

                }).ConfigureAwait(false);
            }
            catch (EventHandlingException ex)
            {
                // Something went wrong in the event handler, fail the scenario.
                scenarioContext.FailException = ex;
            }
        }
    }
}
