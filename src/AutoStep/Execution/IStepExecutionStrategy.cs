using System.Threading.Tasks;

namespace AutoStep.Execution
{
    internal interface IStepExecutionStrategy
    {
        Task ExecuteStep(StepContext context, VariableSet variables, EventManager events, IExecutionStateManager executionManager, IStepCollectionExecutionStrategy stepCollectionExecutionStrategy);
    }

}
