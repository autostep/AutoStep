using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoStep.Definitions;
using AutoStep.Elements.Metadata;
using AutoStep.Execution.Contexts;
using AutoStep.Execution.Dependency;

namespace AutoStep.Execution.Strategy
{
    /// <summary>
    /// Implements the default step execution strategy.
    /// </summary>
    internal class DefaultStepExecutionStrategy : IStepExecutionStrategy
    {
        private const string StepExecutionStack = "__asStepStack";

        /// <summary>
        /// Execute the strategy.
        /// </summary>
        /// <param name="stepScope">The step scope.</param>
        /// <param name="context">The step context.</param>
        /// <param name="variables">The set of variables currently in-scope.</param>
        /// <returns>A task that should complete when the step has finished executing.</returns>
        public async ValueTask ExecuteStep(
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

            var threadContext = stepScope.ThreadContext();

            var stepStack = threadContext.GetOrAdd(StepExecutionStack, () => new Stack<IStepReferenceInfo>());

            if (stepStack.Count > 0)
            {
                if (stepStack.Any(c => c.Binding is object && binding.Definition.IsSameDefinition(c.Binding.Definition)))
                {
                    // Take a copy of the stack (because it's going to get popped on the way up).
                    throw new CircularStepReferenceException(binding.Definition, new List<IStepReferenceInfo>(stepStack));
                }
            }

            try
            {
                stepStack.Push(reference);

                await binding.Definition.ExecuteStepAsync(stepScope, context, variables);
            }
            finally
            {
                stepStack.Pop();
            }
        }
    }
}
