using System;
using System.Collections.Generic;
using System.Text;
using AutoStep.Execution.Logging;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Xunit;

namespace AutoStep.Tests.Execution.Logging
{
    public class LogConsumerTests
    {
        [Fact]
        public void CanRetrieveAllLogs()
        {
            var set = new List<LogEntry>();

            var consumer = new LogConsumer(set);

            consumer.TryGetNextEntry(out var _).Should().BeFalse();

            set.Add(new TestLogEntry("cat", LogLevel.Information, default, null!));
            set.Add(new TestLogEntry("cat", LogLevel.Debug, default, null!));

            consumer.TryGetNextEntry(out var logEntry).Should().BeTrue();
            logEntry!.LogLevel.Should().Be(LogLevel.Information);
            consumer.TryGetNextEntry(out logEntry).Should().BeTrue();
            logEntry!.LogLevel.Should().Be(LogLevel.Debug);

            consumer.TryGetNextEntry(out logEntry).Should().BeFalse();
            logEntry.Should().BeNull();
        }

        [Fact]
        public void CanContinueAfterAdditionalLogs()
        {
            var set = new List<LogEntry>();

            var consumer = new LogConsumer(set);

            consumer.TryGetNextEntry(out var _).Should().BeFalse();

            set.Add(new TestLogEntry("cat", LogLevel.Information, default, null!));
            

            consumer.TryGetNextEntry(out var logEntry).Should().BeTrue();
            logEntry!.LogLevel.Should().Be(LogLevel.Information);

            consumer.TryGetNextEntry(out var _).Should().BeFalse();

            set.Add(new TestLogEntry("cat", LogLevel.Debug, default, null!));

            consumer.TryGetNextEntry(out logEntry).Should().BeTrue();
            logEntry!.LogLevel.Should().Be(LogLevel.Debug);

            consumer.TryGetNextEntry(out logEntry).Should().BeFalse();
            logEntry.Should().BeNull();
        }

        private class TestLogEntry : LogEntry
        {
            public TestLogEntry(string categoryName, LogLevel logLevel, EventId eventId, Exception exception) : base(categoryName, logLevel, eventId, exception)
            {
            }

            public override string Text => LogLevel.ToString();
        }
    }
}
