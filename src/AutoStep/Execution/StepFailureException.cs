using System;
using System.Collections.Generic;
using System.Text;
using AutoStep.Elements;
using AutoStep.Elements.ReadOnly;

namespace AutoStep.Execution
{
    public class StepFailureException : Exception
    {
        public IStepReferenceInfo Step { get; }

        public StepFailureException(IStepReferenceInfo stepReference, Exception innerException) : base("Step Failure Occurred", innerException)
        {
            Step = stepReference;
        }

        public StepFailureException(IStepReferenceInfo stepReference, string message)
            : base(message)
        {
            Step = stepReference;
        }
    }
}
