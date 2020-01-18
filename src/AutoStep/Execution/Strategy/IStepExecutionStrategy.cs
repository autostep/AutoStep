using System.Threading.Tasks;
using AutoStep.Execution.Control;

namespace AutoStep.Execution.Strategy
{
    internal interface IStepExecutionStrategy
    {
        Task ExecuteStep(StepContext context, VariableSet variables, EventManager events, IExecutionStateManager executionManager, IStepCollectionExecutionStrategy stepCollectionExecutionStrategy);
    }

}
