using System;
using AutoStep.Execution.Events;

namespace AutoStep.Execution
{
    /// <summary>
    /// Represents an error that happened inside an event handler, as opposed to an error with an actual step.
    /// </summary>
    public class EventHandlingException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EventHandlingException"/> class.
        /// </summary>
        /// <param name="handler">The handler that failed.</param>
        /// <param name="innerException">The underlying exception.</param>
        public EventHandlingException(IEventHandler handler, Exception innerException)
            : base(ExecutionText.EventHandlingException_Message, innerException)
        {
            FailedEventHandler = handler;
        }

        public IEventHandler FailedEventHandler { get; }
    }
}
