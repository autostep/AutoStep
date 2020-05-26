using System;
using Microsoft.Extensions.Logging;

namespace AutoStep.Execution.Logging
{
    /// <summary>
    /// Defines a log entry typed to a state object.
    /// </summary>
    /// <typeparam name="TState">The state type.</typeparam>
    public class LogEntry<TState> : LogEntry
    {
        private readonly Func<TState, Exception?, string> formatter;
        private string? renderedText;

        /// <summary>
        /// Initializes a new instance of the <see cref="LogEntry{TState}"/> class.
        /// </summary>
        /// <param name="categoryName">The category name.</param>
        /// <param name="logLevel">The log level.</param>
        /// <param name="eventId">The event id.</param>
        /// <param name="state">The log state object.</param>
        /// <param name="exception">An optional exception for error events.</param>
        /// <param name="formatter">A formatter callback.</param>
        public LogEntry(string categoryName, LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            : base(categoryName, logLevel, eventId, exception)
        {
            State = state;
            this.formatter = formatter;
        }

        /// <summary>
        /// Gets the state object related to the log event.
        /// </summary>
        public TState State { get; }

        /// <inheritdoc/>
        public override string Text => renderedText ??= formatter(State, Exception);
    }
}
