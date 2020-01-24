using System.Threading.Tasks;
using AutoStep.Execution.Contexts;
using AutoStep.Execution.Dependency;

namespace AutoStep.Execution.Strategy
{
    /// <summary>
    /// Defines the interface for a step execution strategy, that controls the behaviour of executing a single step.
    /// </summary>
    public interface IStepExecutionStrategy
    {
        /// <summary>
        /// Execute the strategy.
        /// </summary>
        /// <param name="stepScope">The step scope.</param>
        /// <param name="context">The step context.</param>
        /// <param name="variables">The set of variables currently in-scope.</param>
        /// <returns>A task that should complete when the step has finished executing.</returns>
        ValueTask ExecuteStep(
                    IServiceScope stepScope,
                    StepContext context,
                    VariableSet variables);
    }
}
