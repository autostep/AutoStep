using System;
using System.Runtime.Serialization;
using AutoStep.Elements;
using AutoStep.Elements.ReadOnly;

namespace AutoStep.Execution
{
    internal class UnboundStepException : StepFailureException
    {
        public UnboundStepException(IStepReferenceInfo stepReference)
            : base(stepReference, "Step has not been bound.")
        {
        }
    }
}
