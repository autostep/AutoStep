using System;
using AutoStep.Execution.Contexts;
using AutoStep.Execution.Logging;
using AutoStep.Tests.Utils;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace AutoStep.Tests.Execution.Logging
{
    public class CapturingLoggerTests
    {
        [Fact]
        public void LoggerCapturesLogEntry()
        {
            var contextProvider = new ContextScopeProvider();

            var threadContext = new ThreadContext(1);

            string? forwardedLog = null;

            var delegatingLogger = new DelegatingLogger(log =>
            {
                forwardedLog = log;
            });

            var loggerWrapper = new CapturingLogger(contextProvider, "category", delegatingLogger);

            using (contextProvider.EnterContextScope(threadContext))
            {
                loggerWrapper.LogInformation("Message: {0}", 123);
            }

            threadContext.AllLogs[0].Text.Should().Be("Message: 123");
            forwardedLog.Should().Be("Message: 123");
        }


        [Fact]
        public void LoggerDoesNotRequireContext()
        {
            var contextProvider = new ContextScopeProvider();

            string? forwardedLog = null;

            var delegatingLogger = new DelegatingLogger(log =>
            {
                forwardedLog = log;
            });

            var loggerWrapper = new CapturingLogger(contextProvider, "category", delegatingLogger);

            // This should not throw.
            loggerWrapper.LogInformation("Message: {0}", 123);

            forwardedLog.Should().Be("Message: 123");
        }

        private class DelegatingLogger : ILogger
        {
            private readonly Action<string> logCallback;

            public DelegatingLogger(Action<string> logCallback)
            {
                this.logCallback = logCallback;
            }

            public IDisposable BeginScope<TState>(TState state)
            {
                throw new NotImplementedException();
            }

            public bool IsEnabled(LogLevel logLevel)
            {
                throw new NotImplementedException();
            }

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                logCallback(formatter(state, exception));
            }
        }
    }
}
