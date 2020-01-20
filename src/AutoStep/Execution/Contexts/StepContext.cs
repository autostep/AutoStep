using AutoStep.Elements.ReadOnly;

namespace AutoStep.Execution
{

    public class StepContext : ErrorCapturingContext
    {
        public StepContext(int stepIndex, ErrorCapturingContext parentContext, IStepReferenceInfo step, VariableSet variables)
        {
            StepIndex = stepIndex;
            ParentContext = parentContext;
            Step = step;
            Variables = variables;
        }

        public int StepIndex { get; }

        public ErrorCapturingContext ParentContext { get; }

        public IStepReferenceInfo Step { get; }

        public VariableSet Variables { get; }
    }
}
