using System;
using Microsoft.Extensions.Logging;

namespace AutoStep.Execution.Logging
{
    /// <summary>
    /// Defines a single log entry, raised by a <see cref="ILogger"/> during a test.
    /// </summary>
    public abstract class LogEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LogEntry"/> class.
        /// </summary>
        /// <param name="categoryName">The category name.</param>
        /// <param name="logLevel">The log level.</param>
        /// <param name="eventId">The event id.</param>
        /// <param name="exception">An optional exception for error events.</param>
        protected LogEntry(string categoryName, LogLevel logLevel, EventId eventId, Exception? exception)
        {
            CategoryName = categoryName;
            LogLevel = logLevel;
            EventId = eventId;
            Exception = exception;
        }

        /// <summary>
        /// Gets the category name for the logger that this event came from.
        /// </summary>
        public string CategoryName { get; }

        /// <summary>
        /// Gets the log level of the event.
        /// </summary>
        public LogLevel LogLevel { get; }

        /// <summary>
        /// Gets the event ID of the event (if there is one).
        /// </summary>
        public EventId EventId { get; }

        /// <summary>
        /// Gets the optional exception for this log entry.
        /// </summary>
        public Exception? Exception { get; }

        /// <summary>
        /// Gets the text content of the log entry.
        /// </summary>
        public abstract string Text { get; }
    }
}
