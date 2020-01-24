using System.Threading.Tasks;
using AutoStep.Execution.Contexts;
using AutoStep.Execution.Dependency;

namespace AutoStep.Execution.Strategy
{
    /// <summary>
    /// Implements the default step execution strategy.
    /// </summary>
    internal class DefaultStepExecutionStrategy : IStepExecutionStrategy
    {
        /// <summary>
        /// Execute the strategy.
        /// </summary>
        /// <param name="stepScope">The step scope.</param>
        /// <param name="context">The step context.</param>
        /// <param name="variables">The set of variables currently in-scope.</param>
        /// <returns>A task that should complete when the step has finished executing.</returns>
        public ValueTask ExecuteStep(
            IServiceScope stepScope,
            StepContext context,
            VariableSet variables)
        {
            var reference = context.Step;
            var binding = reference.Binding;

            if (binding is null)
            {
                throw new UnboundStepException(reference);
            }

            return binding.Definition.ExecuteStepAsync(stepScope, context, variables);
        }
    }
}
