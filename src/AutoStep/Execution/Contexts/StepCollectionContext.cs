using System;
using AutoStep.Elements.ReadOnly;

namespace AutoStep.Execution.Contexts
{
    public class StepCollectionContext : TestExecutionContext
    {
        public Exception? FailException { get; set; }

        public IStepReferenceInfo? FailingStep { get; set; }

        public TimeSpan Elapsed { get; internal set; }
    }
}
