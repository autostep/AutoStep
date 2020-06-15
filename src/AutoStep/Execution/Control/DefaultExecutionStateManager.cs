using System;
using System.Threading.Tasks;
using Autofac;
using AutoStep.Execution.Contexts;
using AutoStep.Execution.Dependency;

namespace AutoStep.Execution.Control
{
    /// <summary>
    /// Default execution state manager; never pauses execution.
    /// </summary>
    public class DefaultExecutionStateManager : IExecutionStateManager
    {
        /// <inheritdoc/>
        public ValueTask<HaltResponseInstruction?> CheckforHalt(ILifetimeScope scope, TestExecutionContext context, TestThreadState starting)
        {
            return default;
        }

        /// <inheritdoc/>
        public ValueTask<BreakResponseInstruction?> StepError(StepContext stepContext)
        {
            return default;
        }
    }
}
