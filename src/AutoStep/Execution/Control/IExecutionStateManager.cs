using System.Threading.Tasks;

namespace AutoStep.Execution.Control
{
    /// <summary>
    /// Allows the RunContext to interact with a state manager that can control program flow (step over, step into, pause, etc).
    /// It's a bit like an attached debugger. There will be a 'default' execution state manager that just says 'continue' for everything,
    /// which is basically like just running a test.
    /// </summary>
    public interface IExecutionStateManager
    {
        Task<HaltResponseInstruction?> CheckforHalt(ExecutionContext context, TestThreadState starting);
        Task<BreakResponseInstruction?> StepError(StepContext stepContext);
    }
}
