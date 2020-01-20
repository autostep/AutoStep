using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;

namespace AutoStep.Tracing
{
    internal class LoggerWrapper<T> : ILogger
    {
        private ILogger logImplementation;

        public LoggerWrapper(ILoggerFactory logFactory)
        {
            logImplementation = logFactory.CreateLogger<T>();
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return logImplementation.BeginScope(state);
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logImplementation.IsEnabled(logLevel);
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            logImplementation.Log(logLevel, eventId, state, exception, formatter);
        }
    }
}
