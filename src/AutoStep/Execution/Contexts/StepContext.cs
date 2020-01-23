using System;
using AutoStep.Elements.ReadOnly;

namespace AutoStep.Execution.Contexts
{

    public class StepContext : TestExecutionContext
    {
        public StepContext(int stepIndex, StepCollectionContext parentContext, IStepReferenceInfo step, VariableSet variables)
        {
            StepIndex = stepIndex;
            ParentContext = parentContext;
            Step = step;
            Variables = variables;
        }

        public int StepIndex { get; }

        public StepCollectionContext ParentContext { get; }

        public IStepReferenceInfo Step { get; }

        public VariableSet Variables { get; }

        public Exception? FailException { get; set; }

        public TimeSpan Elapsed { get; internal set; }
    }
}
