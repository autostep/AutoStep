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

    /// <summary>
    /// Default execution state manager does things.
    /// </summary>
    public class DefaultExecutionStateManager : IExecutionStateManager
    {
        private readonly Task<HaltResponseInstruction?> Completed = Task.FromResult<HaltResponseInstruction?>(null);

        public Task<HaltResponseInstruction?> CheckforHalt(ThreadContext threadCtxt)
        {
            return Completed;
        }
    }
}
