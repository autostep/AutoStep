using System.Threading.Tasks;
using AutoStep.Execution.Control;
using AutoStep.Execution.Dependency;

namespace AutoStep.Execution.Strategy
{
    internal interface IStepExecutionStrategy
    {
        Task ExecuteStep(
            IServiceScope stepScope,
            StepContext context,
            VariableSet variables,
            EventPipeline events,
            IExecutionStateManager executionManager,
            IStepCollectionExecutionStrategy stepCollectionExecutionStrategy);
    }

}
