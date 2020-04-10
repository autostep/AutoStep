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
        /// <returns>A task that should complete when the scenario has finished executing.</returns>
        public async ValueTask Execute(IAutoStepServiceScope featureScope, FeatureContext featureContext, IScenarioInfo scenario, VariableSet variableSet)
        {
            var scenarioContext = new ScenarioContext(scenario, variableSet);

            var collectionExecutor = featureScope.GetRequiredService<IStepCollectionExecutionStrategy>();
            var executionManager = featureScope.GetRequiredService<IExecutionStateManager>();
            var events = featureScope.GetRequiredService<IEventPipeline>();

            using var scenarioScope = featureScope.BeginNewScope(ScopeTags.ScenarioTag, scenarioContext);

            // Halt before the scenario begins.
            var haltInstruction = await executionManager.CheckforHalt(scenarioScope, scenarioContext, TestThreadState.StartingScenario).ConfigureAwait(false);

            try
            {
                await events.InvokeEvent(scenarioScope, scenarioContext, (handler, sc, ctxt, next) => handler.OnScenario(sc, ctxt, next), async (_, ctxt) =>
                {
                    scenarioContext.ScenarioRan = true;

                    if (featureContext.Feature.Background is object)
                    {
                        // There is a background to execute.
                        await collectionExecutor.Execute(
                                scenarioScope,
                                ctxt,
                                featureContext.Feature.Background,
                                variableSet).ConfigureAwait(false);

                        if (ctxt.FailException is object)
                        {
                            // Something went wrong executing the background. Don't run the main scenario body.
                            return;
                        }
                    }

                    // Any errors will be updated on the scenario context.
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
