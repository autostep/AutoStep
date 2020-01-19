using AutoStep.Elements;

namespace AutoStep.Execution
{

    public class StepContext : ErrorCapturingContext
    {
        internal StepContext(int stepIndex, ErrorCapturingContext parentContext, StepReferenceElement step, VariableSet variables)
        {
            StepIndex = stepIndex;
            ParentContext = parentContext;
            Step = step;
            Variables = variables;
        }

        public int StepIndex { get; }

        public ErrorCapturingContext ParentContext { get; }

        public StepReferenceElement Step { get; }

        public VariableSet Variables { get; }
    }
}
