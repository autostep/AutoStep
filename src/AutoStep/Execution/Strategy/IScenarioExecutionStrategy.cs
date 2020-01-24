using System.Threading.Tasks;
using AutoStep.Elements.Metadata;
using AutoStep.Execution.Contexts;
using AutoStep.Execution.Dependency;

namespace AutoStep.Execution.Strategy
{
    /// <summary>
    /// Defines the interface for a scenario execution strategy, that controls the behaviour of executing a single scenario/example combo.
    /// </summary>
    public interface IScenarioExecutionStrategy
    {
        /// <summary>
        /// Execute the strategy.
        /// </summary>
        /// <param name="featureScope">The current service scope (which will be a feature scope).</param>
        /// <param name="featureContext">The current feature context.</param>
        /// <param name="scenario">The scenario metadata.</param>
        /// <param name="variables">The set of variables currently in-scope.</param>
        /// <returns>A task that should complete when the scenario has finished executing.</returns>
        ValueTask Execute(
            IServiceScope featureScope,
            FeatureContext featureContext,
            IScenarioInfo scenario,
            VariableSet variables);
    }
}
