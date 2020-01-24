using System;
using Microsoft.Extensions.Logging;

namespace AutoStep.Logging
{
    /// <summary>
    /// Defines a simple logger wrapper that allows us to register a microsoft logging implementation in our DI
    /// container.
    /// </summary>
    /// <typeparam name="T">The logger type.</typeparam>
    internal class LoggerWrapper<T> : ILogger<T>
    {
        private ILogger logImplementation;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoggerWrapper{T}"/> class.
        /// </summary>
        /// <param name="logFactory">The log factory.</param>
        public LoggerWrapper(ILoggerFactory logFactory)
        {
            logImplementation = logFactory.CreateLogger<T>();
        }

        /// <inheritdoc/>
        public IDisposable BeginScope<TState>(TState state)
        {
            return logImplementation.BeginScope(state);
        }

        /// <inheritdoc/>
        public bool IsEnabled(LogLevel logLevel)
        {
            return logImplementation.IsEnabled(logLevel);
        }

        /// <inheritdoc/>
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            logImplementation.Log(logLevel, eventId, state, exception, formatter);
        }
    }
}
