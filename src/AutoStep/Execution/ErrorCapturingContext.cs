using System;

namespace AutoStep.Execution
{

    public class ErrorCapturingContext : ExecutionContext
    {
        public Exception? FailException { get; internal set; }

        public TimeSpan Elapsed { get; internal set; }
    }
}
