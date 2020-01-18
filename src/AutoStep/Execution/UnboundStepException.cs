using System;
using System.Runtime.Serialization;
using AutoStep.Elements;

namespace AutoStep.Execution
{
    internal class UnboundStepException : StepFailureException
    {
        public UnboundStepException(StepReferenceElement stepReference)
            : base(stepReference, "Step has not been bound.")
        {
        }
    }
}
