using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using AutoStep.Execution.Contexts;

namespace AutoStep.Execution.Logging
{
    /// <summary>
    /// Provides a wrapper around a set of log entries that ensures log entries are consumed in order and are not re-used for a given consumer.
    /// </summary>
    public sealed class LogConsumer
    {
        private readonly IReadOnlyList<LogEntry> backingSet;
        private int nextPosition;

        /// <summary>
        /// Initializes a new instance of the <see cref="LogConsumer"/> class.
        /// </summary>
        /// <param name="backingSet">The underlying set of log entries.</param>
        public LogConsumer(IReadOnlyList<LogEntry> backingSet)
        {
            this.backingSet = backingSet;
            nextPosition = 0;
        }

        /// <summary>
        /// Attempt to retrieve the next log entry in the sequence.
        /// </summary>
        /// <param name="logEntry">The next log entry.</param>
        /// <returns>True if a log entry was retrieved, false if there are no log entries available.</returns>
        public bool TryGetNextEntry([NotNullWhen(true)] out LogEntry? logEntry)
        {
            if (backingSet.Count > nextPosition)
            {
                logEntry = backingSet[nextPosition++];
                return true;
            }

            logEntry = null;
            return false;
        }
    }
}
