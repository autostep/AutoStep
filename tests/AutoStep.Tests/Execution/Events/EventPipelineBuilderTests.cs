using System;
using System.Threading;
using System.Threading.Tasks;
using AutoStep.Execution.Contexts;
using AutoStep.Execution.Dependency;
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

            builder.Invoking(b => b.Add(null!)).Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void BuildCreatesEventPipeline()
        {
            var builder = new EventPipelineBuilder();

            var mock = new Mock<IEventHandler>();
            mock.Setup(x => x.OnExecuteAsync(null!, null!, null!, CancellationToken.None)).Verifiable();
            var evHandler = mock.Object;

            builder.Add(evHandler);

            var pipeline = builder.Build();

            pipeline.InvokeEventAsync<RunContext>(null!, null!, (h, s, c, n, cancel) => h.OnExecuteAsync(s, c, n, cancel), CancellationToken.None);

            mock.Verify(x => x.OnExecuteAsync(null!, null!, It.IsAny<Func<IServiceProvider, RunContext, CancellationToken, ValueTask>>(), CancellationToken.None), Times.Once());
        }
    }
}
