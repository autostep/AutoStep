using AutoStep.Elements.Metadata;

namespace AutoStep.Execution
{
    /// <summary>
    /// Thrown by the step execution strategy if a step has not been bound. This is typically the sort of thing that should be caught at link
    /// time.
    /// </summary>
    public class UnboundStepException : StepFailureException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnboundStepException"/> class.
        /// </summary>
        /// <param name="stepReference">The unbound step reference.</param>
        public UnboundStepException(IStepReferenceInfo stepReference)
            : base(stepReference, ExecutionText.UnboundStepException)
        {
        }
    }
}
