using System;

namespace AutoStep.Execution.Contexts
{
    public class ErrorCapturingContext : TestExecutionContext
    {
        public TimeSpan Elapsed { get; internal set; }
    }
}
