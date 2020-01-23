using System.Threading.Tasks;
using AutoStep.Execution.Contexts;
using AutoStep.Execution.Control;
using AutoStep.Execution.Dependency;

namespace AutoStep.Execution.Strategy
{
    public interface IStepExecutionStrategy
    {
        ValueTask ExecuteStep(
                    IServiceScope stepScope,
                    StepContext context,
                    VariableSet variables);
    }
}
