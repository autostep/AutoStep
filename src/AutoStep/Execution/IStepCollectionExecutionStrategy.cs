using System.Threading.Tasks;
using AutoStep.Elements;

namespace AutoStep.Execution
{
    internal interface IStepCollectionExecutionStrategy
    {
        Task Execute(ErrorCapturingContext owningContext, StepCollectionElement stepCollection, VariableSet variables, EventManager events, IExecutionStateManager executionManager);
    }
}
