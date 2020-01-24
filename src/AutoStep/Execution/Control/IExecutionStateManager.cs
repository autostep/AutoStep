using System.Threading.Tasks;
using AutoStep.Execution.Contexts;
using AutoStep.Execution.Dependency;

namespace AutoStep.Execution.Control
{
    /// <summary>
    /// Allows the RunContext to interact with a state manager that can control program flow (step over, step into, pause, etc).
    /// It's a bit like an attached debugger. There will be a 'default' execution state manager that just says 'continue' for everything,
    /// which is basically like just running a test.
    /// </summary>
    public interface IExecutionStateManager
    {
        /// <summary>
        /// Asks the execution state manager if execution should halt at the point specified by the parameters.
        /// </summary>
        /// <param name="scope">The current scope of execution.</param>
        /// <param name="context">The current context.</param>
        /// <param name="starting">An enum indicating which stage of execution is happening.</param>
        /// <returns>An instruction to the calling thread.</returns>
        ValueTask<HaltResponseInstruction?> CheckforHalt(IServiceScope scope, TestExecutionContext context, TestThreadState starting);

        /// <summary>
        /// Asks the execution state manager what to do when an error occurs in a step (gives an opportunity to break).
        /// </summary>
        /// <param name="stepContext">The step context.</param>
        /// <returns>An instruction to the calling thread.</returns>
        ValueTask<BreakResponseInstruction?> StepError(StepContext stepContext);
    }
}
