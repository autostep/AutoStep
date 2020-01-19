using System.Threading.Tasks;
using AutoStep.Elements;
using AutoStep.Execution.Control;
using AutoStep.Execution.Dependency;

namespace AutoStep.Execution.Strategy
{
    internal interface IScenarioExecutionStrategy
    {
        Task Execute(
            IServiceScope featureScope,
            FeatureContext featureContext,
            ScenarioElement scenario,
            VariableSet variables,
            EventPipeline events,
            IExecutionStateManager executionManager);
    }
}
