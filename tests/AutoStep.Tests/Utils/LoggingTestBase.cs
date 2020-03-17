using Xunit.Abstractions;
using Microsoft.Extensions.Logging;
using System.Linq;
using System;
using FluentAssertions;
using System.Collections.Concurrent;

namespace AutoStep.Tests.Utils
{
    public class LoggingTestBase
    {
        public LoggingTestBase(ITestOutputHelper outputHelper)
        {
            TestOutput = outputHelper;
            LogFactory = TestLogFactory.Create(outputHelper);
            LoggedMessages = new LogContainer();
            LogFactory.AddProvider(LoggedMessages);
        }

        protected ITestOutputHelper TestOutput { get; }

        protected ILoggerFactory LogFactory { get; }

        protected LogContainer LoggedMessages { get; }

        protected class LogContainer : ILoggerProvider
        {
            public ConcurrentBag<(LogLevel lvl, string msg)> Messages { get; } = new ConcurrentBag<(LogLevel lvl, string msg)>();

            public void LastShouldContain(LogLevel level, params string[] messageContents)
            {
                var last = Messages.Last();

                last.lvl.Should().Be(level);
                last.msg.Should().ContainAll(messageContents);
            }

            public void ShouldContain(LogLevel level, params string[] messageContents)
            {
                var found = Messages.FirstOrDefault(p => p.lvl == level && messageContents.All(m => p.msg.Contains(m)));

                found.Should().NotBeNull("messages ('{0}') should contain {1} - {2}", Messages, level, messageContents);
            }

            public ILogger CreateLogger(string categoryName)
            {
                return new LogMessageSink(this);
            }

            public void Dispose()
            {
            }
        }

        private class LogMessageSink : ILogger
        {
            private readonly LogContainer container;

            public LogMessageSink(LogContainer container)
            {
                this.container = container;
            }

            public IDisposable BeginScope<TState>(TState state)
            {
                throw new NotImplementedException();
            }

            public bool IsEnabled(LogLevel logLevel)
            {
                return true;
            }

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                container.Messages.Add((logLevel, formatter(state, exception)));
            }
        }
    }
}
