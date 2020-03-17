using System;
using AutoStep.Execution.Events;
using FluentAssertions;
using Moq;
using Xunit;

namespace AutoStep.Tests.Execution.Events
{
    public class EventPipelineBuilderTests
    {
        [Fact]
        public void NullEventHandlerThrowsNullArgumentException()
        {
            var builder = new EventPipelineBuilder();

            builder.Invoking(b => b.Add(null)).Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void BuildCreatesEventPipeline()
        {
            var builder = new EventPipelineBuilder();

            var mock = new Mock<IEventHandler>();
            mock.Setup(x => x.ConfigureServices(null, null)).Verifiable();
            var evHandler = mock.Object;

            builder.Add(evHandler);

            var pipeline = builder.Build();

            pipeline.ConfigureServices(null, null);

            mock.Verify(x => x.ConfigureServices(null, null), Times.Once());
        }
    }
}
