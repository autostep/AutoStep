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
            mock.Setup(x => x.OnExecute(null!, null!, null!)).Verifiable();
            var evHandler = mock.Object;

            builder.Add(evHandler);

            var pipeline = builder.Build();

            pipeline.InvokeEvent<RunContext>(null!, null!, (h, s, c, n) => h.OnExecute(s, c, n));

            mock.Verify(x => x.OnExecute(null!, null!, It.IsAny<Func<IServiceProvider, RunContext, ValueTask>>()), Times.Once());
        }
    }
}
