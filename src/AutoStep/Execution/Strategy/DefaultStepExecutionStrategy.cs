using System.Threading.Tasks;
using AutoStep.Execution.Contexts;
using AutoStep.Execution.Control;
using AutoStep.Execution.Dependency;

namespace AutoStep.Execution.Strategy
{
    internal class DefaultStepExecutionStrategy : IStepExecutionStrategy
    {
        public Task ExecuteStep(
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
