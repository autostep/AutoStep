using System.Threading.Tasks;
using AutoStep.Execution.Control;
using AutoStep.Execution.Dependency;

namespace AutoStep.Execution.Strategy
{
    internal class DefaultStepExecutionStrategy : IStepExecutionStrategy
    {
        public Task ExecuteStep(
            IServiceScope stepScope,
            StepContext context,
            VariableSet variables,
            EventPipeline events,
            IExecutionStateManager executionManager,
            IStepCollectionExecutionStrategy collectionExecutor)
        {
            var reference = context.Step;
            var binding = reference.Binding;

            if (binding is null)
            {
                throw new UnboundStepException(reference);
            }

            // Create args structure.
            var args = new StepExecutionArgs(stepScope, context, reference, variables, binding, events, executionManager, collectionExecutor);

            return binding.Definition.ExecuteStepAsync(args);
        }
    }
}
