using System;

namespace AutoStep.Execution
{

    public class ErrorCapturingContext : TestExecutionContext
    {
        public Exception? FailException { get; internal set; }

        public TimeSpan Elapsed { get; internal set; }
    }
}
