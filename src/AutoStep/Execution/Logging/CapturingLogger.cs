using System;
using AutoStep.Execution.Contexts;
using Microsoft.Extensions.Logging;

namespace AutoStep.Execution.Logging
{
    /// <summary>
    /// Defines a simple logger that will store log entries in the active execution context.
    /// container.
    /// </summary>
    internal class CapturingLogger : ILogger
    {
        private readonly IContextScopeProvider contextScopeProvider;
        private readonly string categoryName;
        private readonly ILogger wrappedLogger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CapturingLogger"/> class.
        /// </summary>
        /// <param name="contextScopeProvider">The context scope provider.</param>
        /// <param name="categoryName">The category name.</param>
        /// <param name="wrappedLogger">The wrapped logger (to forward logs to).</param>
        public CapturingLogger(IContextScopeProvider contextScopeProvider, string categoryName, ILogger wrappedLogger)
        {
            this.contextScopeProvider = contextScopeProvider;
            this.categoryName = categoryName;
            this.wrappedLogger = wrappedLogger;
        }

        /// <inheritdoc/>
        public IDisposable BeginScope<TState>(TState state)
        {
            // Some other context; let the wrapped logger see it.
            return wrappedLogger.BeginScope(state);
        }

        /// <inheritdoc/>
        public bool IsEnabled(LogLevel logLevel)
        {
            // Let's look at the execution context.
            var currentContext = contextScopeProvider.Current;

            var wrappedEnabled = wrappedLogger.IsEnabled(logLevel);

            if (currentContext is object)
            {
                // Consider both the capturing settings and the logger we're forwarding to.
                wrappedEnabled |= logLevel >= currentContext.LogCaptureLevel;
            }

            return wrappedEnabled;
        }

        /// <inheritdoc/>
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception?, string> formatter)
        {
            // Let's look at the execution context.
            var currentContext = contextScopeProvider.Current;

            if (currentContext is object)
            {
                currentContext.CaptureLog(categoryName, logLevel, eventId, state, exception, formatter);
            }

            wrappedLogger.Log(logLevel, eventId, state, exception, formatter);
        }
    }
}
