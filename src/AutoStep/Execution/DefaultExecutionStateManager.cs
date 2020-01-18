using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using AutoStep.Elements;
using AutoStep.Projects;
using AutoStep.Tracing;

namespace AutoStep.Execution
{

    public enum ContinueTo
    {
        End,
        NextStep,
        NextScenario,
        NextFeature
    }

    /// <summary>
    /// Tells the calling thread what to do after the halt.
    /// </summary>
    public class HaltResponseInstruction
    {

    }

    public class BreakResponseInstruction
    {

    }

    /// <summary>
    /// Default execution state manager does things.
    /// </summary>
    public class DefaultExecutionStateManager : IExecutionStateManager
    {
        private readonly Task<HaltResponseInstruction?> CompletedHalt = Task.FromResult<HaltResponseInstruction?>(null);
        private readonly Task<BreakResponseInstruction?> CompletedBreak = Task.FromResult<BreakResponseInstruction?>(null);

        public Task<HaltResponseInstruction?> CheckforHalt(ExecutionContext context, TestThreadState starting)
        {
            return CompletedHalt;
        }

        public Task<BreakResponseInstruction?> StepError(StepContext stepContext)
        {
            return CompletedBreak;
        }
    }
}
