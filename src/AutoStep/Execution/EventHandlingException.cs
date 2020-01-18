using System;
using System.Collections.Generic;
using System.Text;
using AutoStep.Elements;

namespace AutoStep.Execution
{

    public class EventHandlingException : Exception
    {
        public EventHandlingException(Exception innerException) : base("Event Handler Failure Occurred", innerException)
        {
        }
    }
}
