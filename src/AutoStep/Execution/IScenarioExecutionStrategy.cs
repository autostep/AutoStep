using System.Threading.Tasks;
using AutoStep.Elements;

namespace AutoStep.Execution
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
