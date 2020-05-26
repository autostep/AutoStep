using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;

namespace AutoStep.Execution.Logging
{
    public abstract class LogEntry
    {
        protected LogEntry(string categoryName, LogLevel logLevel, EventId eventId, Exception exception)
        {
            CategoryName = categoryName;
            LogLevel = logLevel;
            EventId = eventId;
            Exception = exception;
        }

        public string CategoryName { get; }

        public LogLevel LogLevel { get; }

        public EventId EventId { get; }

        public Exception Exception { get; }

        public abstract string Text { get; }
    }

    public class LogEntry<TState> : LogEntry
    {
        private Func<TState, Exception, string> formatter;

        public LogEntry(string categoryName, LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            : base(categoryName, logLevel, eventId, exception)
        {
            State = state;
            this.formatter = formatter;
        }

        public TState State { get; }

        public override string Text => formatter(State, Exception);
    }
}
