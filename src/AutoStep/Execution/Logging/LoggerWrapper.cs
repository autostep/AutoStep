using System;
using AutoStep.Execution.Contexts;
using Microsoft.Extensions.Logging;

namespace AutoStep.Execution.Logging
{
    /// <summary>
    /// Defines a simple logger wrapper that allows us to register a microsoft logging implementation in our DI
    /// container.
    /// </summary>
    /// <typeparam name="T">The logger type.</typeparam>
    internal class LoggerWrapper : ILogger
    {
        private readonly IContextScopeProvider contextScopeProvider;
        private readonly string categoryName;
        private readonly ILogger wrappedLogger;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoggerWrapper"/> class.
        /// </summary>
        /// <param name="contextScopeProvider">The log factory.</param>
        public LoggerWrapper(IContextScopeProvider contextScopeProvider, string categoryName, ILogger wrappedLogger)
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
            return wrappedLogger.IsEnabled(logLevel);
        }

        /// <inheritdoc/>
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            // Let's look at the execution context.
            var currentContext = contextScopeProvider.Current;

            if (currentContext is object)
            {
                currentContext.Log(categoryName, logLevel, eventId, state, exception, formatter);
            }

            wrappedLogger.Log(logLevel, eventId, state, exception, formatter);
        }
    }
}
