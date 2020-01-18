using System.Threading.Tasks;

namespace AutoStep.Execution
{
    internal class DefaultStepExecutionStrategy : IStepExecutionStrategy
    {
        public Task ExecuteStep(StepContext context, VariableSet variables, EventManager events, IExecutionStateManager executionManager, IStepCollectionExecutionStrategy collectionExecutor)
        {
            var reference = context.Step;
            var binding = reference.Binding;

            if (binding is null)
            {
                throw new UnboundStepException(reference);
            }

            // Create args structure.
            var args = new StepExecutionArgs(context, reference, variables, binding, events, executionManager, collectionExecutor);

            return binding.Definition.ExecuteStepAsync(args);
        }
    }
}
