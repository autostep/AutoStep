using System.Threading.Tasks;
using AutoStep.Elements;
using AutoStep.Elements.ReadOnly;
using AutoStep.Execution.Contexts;
using AutoStep.Execution.Control;
using AutoStep.Execution.Dependency;

namespace AutoStep.Execution.Strategy
{
    public interface IScenarioExecutionStrategy
    {
        Task Execute(
            IServiceScope featureScope,
            FeatureContext featureContext,
            IScenarioInfo scenario,
            VariableSet variables);
    }
}
