using System.Threading.Tasks;
using AutoStep.Elements;
using AutoStep.Elements.ReadOnly;
using AutoStep.Execution.Contexts;
using AutoStep.Execution.Control;
using AutoStep.Execution.Dependency;

namespace AutoStep.Execution.Strategy
{
    public interface IStepCollectionExecutionStrategy
    {
        ValueTask Execute(
                    IServiceScope owningScope,
                    StepCollectionContext owningContext,
                    IStepCollectionInfo stepCollection,
                    VariableSet variables);
    }
}
