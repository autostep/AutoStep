using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace AutoStep.Tests.Utils
{
    public class TestLogProvider : ILoggerProvider
    {
        private readonly ITestOutputHelper outputHelper;

        private ConcurrentDictionary<string, ILogger> loggers = new ConcurrentDictionary<string, ILogger>();

        public TestLogProvider(ITestOutputHelper outputHelper)
        {
            this.outputHelper = outputHelper;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return loggers.GetOrAdd(categoryName, (n) => new TestLogger(this, n));
        }

        public void Log<TState>(string name, LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            outputHelper.WriteLine($"{name} : {logLevel} : {formatter(state, exception)}");
        }

        public void Dispose()
        {
        }
    }
}
