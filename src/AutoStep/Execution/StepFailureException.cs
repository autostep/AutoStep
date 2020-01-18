using System;
using System.Collections.Generic;
using System.Text;

namespace AutoStep.Execution
{
    public class StepFailureException : Exception
    {
        public StepFailureException(Exception innerException) : base("Step Failure Occurred", innerException)
        {
        }
    }

    public class EventHandlingException : Exception
    {
        public EventHandlingException(Exception innerException) : base("Event Handler Failure Occurred", innerException)
        {
        }
    }
}
