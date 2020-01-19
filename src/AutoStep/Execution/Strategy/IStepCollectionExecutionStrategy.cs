using System.Threading.Tasks;
using AutoStep.Elements;
using AutoStep.Execution.Control;
using AutoStep.Execution.Dependency;

namespace AutoStep.Execution.Strategy
{
    internal interface IStepCollectionExecutionStrategy
    {
        Task Execute(
            IServiceScope owningScope,
            ErrorCapturingContext owningContext,
            StepCollectionElement stepCollection,
            VariableSet variables,
            EventPipeline events,
            IExecutionStateManager executionManager);
    }
}
