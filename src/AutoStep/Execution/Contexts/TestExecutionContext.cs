using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using AutoStep.Execution.Logging;
using Microsoft.Extensions.Logging;

namespace AutoStep.Execution.Contexts
{
    /// <summary>
    /// Defines the base test execution context type, from which all contexts derive.
    /// </summary>
    public abstract class TestExecutionContext
    {
        private readonly ConcurrentDictionary<string, object> contextValues = new ConcurrentDictionary<string, object>();
        private readonly ConcurrentQueue<LogEntry> logEntries = new ConcurrentQueue<LogEntry>();

        /// <summary>
        /// Get a value from the context.
        /// </summary>
        /// <typeparam name="TValue">The expected type of the value.</typeparam>
        /// <param name="name">The name of the value.</param>
        /// <returns>The value.</returns>
        public TValue Get<TValue>(string name)
        {
            if (contextValues.TryGetValue(name, out object foundValue))
            {
                if (foundValue is TValue result)
                {
                    return result;
                }

                throw new InvalidOperationException(ExecutionText.TextExecutionContext_NotExpectedType.FormatWith(name, foundValue.GetType().Name));
            }

            throw new KeyNotFoundException(ExecutionText.TextExecutionContext_NotFound.FormatWith(name));
        }

        /// <summary>
        /// Try to retrieve a value from the context.
        /// </summary>
        /// <typeparam name="TValue">The expected type of the value.</typeparam>
        /// <param name="name">The name of the value.</param>
        /// <param name="val">A variable to receive the value.</param>
        /// <returns>True if the value exists and is of the correct type.</returns>
        public bool TryGet<TValue>(string name, out TValue val)
        {
            if (contextValues.TryGetValue(name, out object foundValue))
            {
                if (foundValue is TValue result)
                {
                    val = result;

                    return true;
                }
            }

            val = default!;
            return false;
        }

        public LogLevel LogCaptureLevel { get; set; } = LogLevel.Information;

        public void Log<TState>(string categoryName, LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (logLevel >= LogCaptureLevel)
            {
                // Store the log entry if it meets the threshold.
                logEntries.Enqueue(new LogEntry<TState>(categoryName, logLevel, eventId, state, exception, formatter));
            }
        }

        public bool TryGetNextLogEntry([NotNullWhen(true)] out LogEntry? logEntry)
        {
            return logEntries.TryDequeue(out logEntry);
        }

        /// <summary>
        /// Try to retrieve a value from the context.
        /// </summary>
        /// <typeparam name="TValue">The expected type of the value.</typeparam>
        /// <param name="name">The name of the value.</param>
        /// <param name="valueToAddWhenNotPresent">A function that will return a new value to return.</param>
        /// <returns>True if the value exists and is of the correct type.</returns>
        public TValue GetOrAdd<TValue>(string name, Func<TValue> valueToAddWhenNotPresent)
            where TValue : notnull
        {
            valueToAddWhenNotPresent = valueToAddWhenNotPresent.ThrowIfNull(nameof(valueToAddWhenNotPresent));

            var foundValue = contextValues.GetOrAdd(name, k => valueToAddWhenNotPresent());

            if (foundValue is TValue result)
            {
                return result;
            }

            throw new InvalidOperationException(ExecutionText.TextExecutionContext_NotExpectedType.FormatWith(name, foundValue.GetType().Name));
        }

        /// <summary>
        /// Set a value in the context.
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="name">The name of the value.</param>
        /// <param name="value">The value to store.</param>
        public void Set<TValue>(string name, TValue value)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException(ExecutionText.TextExecutionContext_NameCannotBeEmpty, nameof(name));
            }

            if (value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            contextValues.AddOrUpdate(name, value, (k, v) => v);
        }
    }
}
