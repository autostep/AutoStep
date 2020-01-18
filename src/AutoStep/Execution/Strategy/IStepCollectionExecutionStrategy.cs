using System.Threading.Tasks;
using AutoStep.Elements;
using AutoStep.Execution.Control;

namespace AutoStep.Execution.Strategy
{
    internal interface IStepCollectionExecutionStrategy
    {
        Task Execute(ErrorCapturingContext owningContext, StepCollectionElement stepCollection, VariableSet variables, EventManager events, IExecutionStateManager executionManager);
    }
}
