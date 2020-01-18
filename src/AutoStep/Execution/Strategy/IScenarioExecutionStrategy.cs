using System.Threading.Tasks;
using AutoStep.Elements;
using AutoStep.Execution.Control;

namespace AutoStep.Execution.Strategy
{
    internal interface IScenarioExecutionStrategy
    {
        Task Execute(
            FeatureContext featureContext,
            ScenarioElement scenario,
            VariableSet variables,
            EventManager events,
            IExecutionStateManager executionManager);
    }
}
