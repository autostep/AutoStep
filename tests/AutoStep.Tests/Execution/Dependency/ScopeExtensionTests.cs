using System;
using Autofac;
using Autofac.Core;
using AutoStep.Execution.Contexts;
using AutoStep.Execution.Dependency;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace AutoStep.Tests.Execution.Dependency
{
    public class ScopeExtensionTests
    {
        [Fact]
        public void ContextInstanceRegisteredInNewScope()
        {
            var autofacBuilder = new ContainerBuilder();
            var container = autofacBuilder.Build();

            var contextObject = new MyContext();

            var derived = container.BeginContextScope(ScopeTags.RunTag, contextObject);

            derived.Tag.Should().Be(ScopeTags.RunTag);

            derived.Resolve<MyContext>().Should().Be(contextObject);
        }

        private interface IInterface1
        {
        }

        private interface INotUsed
        {
        }

        private class MyService : IInterface1
        {
        }

        private class MyContext : TestExecutionContext
        {
        }
    }
}
