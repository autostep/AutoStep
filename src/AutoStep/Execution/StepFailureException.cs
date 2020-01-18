using System;
using System.Collections.Generic;
using System.Text;
using AutoStep.Elements;

namespace AutoStep.Execution
{
    public class StepFailureException : Exception
    {
        public StepReferenceElement Step { get; }

        public StepFailureException(StepReferenceElement stepReference, Exception innerException) : base("Step Failure Occurred", innerException)
        {
            Step = stepReference;
        }

        public StepFailureException(StepReferenceElement stepReference, string message)
            : base(message)
        {
            Step = stepReference;
        }
    }
}
