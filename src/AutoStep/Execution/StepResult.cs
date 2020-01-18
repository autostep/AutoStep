using System;
using AutoStep.Elements;

namespace AutoStep.Execution
{
    public class StepResult
    {
        public StepResult(StepReferenceElement refElement, TimeSpan executionTime, Exception? failException = null)
        {
            Step = refElement;
            ExecutionTime = executionTime;
            FailException = failException;
        }

        public StepReferenceElement Step { get; }

        public TimeSpan ExecutionTime { get; }

        public Exception? FailException { get; }
    }
}
