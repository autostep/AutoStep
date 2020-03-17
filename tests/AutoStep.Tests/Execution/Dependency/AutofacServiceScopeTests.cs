using Autofac;
using Autofac.Core;
using AutoStep.Execution.Contexts;
using AutoStep.Execution.Dependency;
using FluentAssertions;
using Moq;
using Xunit;

namespace AutoStep.Tests.Execution.Dependency
{
    public class AutofacServiceScopeTests
    {
        [Fact]
        public void CanResolveService()
        {
            var autofacBuilder = new ContainerBuilder();
            var testInstance = new MyService();

            autofacBuilder.RegisterInstance(testInstance);

            var container = autofacBuilder.Build();

            var scope = new AutofacServiceScope("mytag", container);

            scope.Resolve<MyService>().Should().Be(testInstance);
        }

        [Fact]
        public void ResolveErrorWrapsAutofacError()
        {
            var autofacBuilder = new ContainerBuilder();
            var container = autofacBuilder.Build();

            var scope = new AutofacServiceScope("mytag", container);

            scope.Invoking(s => s.Resolve<MyService>()).Should().Throw<DependencyException>()
                                                                .WithInnerException<DependencyResolutionException>();
        }

        [Fact]
        public void CanCastFromType()
        {
            var autofacBuilder = new ContainerBuilder();
            var testInstance = new MyService();

            autofacBuilder.RegisterInstance(testInstance);

            var container = autofacBuilder.Build();

            var scope = new AutofacServiceScope("mytag", container);

            scope.Resolve<IInterface1>(typeof(MyService)).Should().Be(testInstance);
        }

        [Fact]
        public void DependencyExceptionOnBadCast()
        {
            var autofacBuilder = new ContainerBuilder();
            var testInstance = new MyService();

            autofacBuilder.RegisterInstance(testInstance);

            var container = autofacBuilder.Build();

            var scope = new AutofacServiceScope("mytag", container);

            scope.Invoking(s => s.Resolve<INotUsed>(typeof(MyService))).Should()
                .Throw<DependencyException>()
                .WithMessage("Cannot resolve service of type 'MyService'; it is not assignable to 'INotUsed'.");
        }

        [Fact]
        public void ContextInstanceRegisteredInNewScope()
        {
            var autofacBuilder = new ContainerBuilder();
            var container = autofacBuilder.Build();

            var rootScope = new AutofacServiceScope("mytag", container);
            var contextObject = new MyContext();

            var derived = rootScope.BeginNewScope(ScopeTags.RunTag, contextObject);

            derived.Tag.Should().Be(ScopeTags.RunTag);

            derived.Resolve<MyContext>().Should().Be(contextObject);
        }

        [Fact]
        public void ContextInstanceRegisteredInNewUntaggedScope()
        {
            var autofacBuilder = new ContainerBuilder();
            var container = autofacBuilder.Build();

            var rootScope = new AutofacServiceScope("mytag", container);
            var contextObject = new MyContext();

            var derived = rootScope.BeginNewScope(contextObject);

            derived.Tag.Should().Be(ScopeTags.GeneralScopeTag);
            derived.Resolve<MyContext>().Should().Be(contextObject);
        }

        [Fact]
        public void LifetimeScopeDisposed()
        {
            var mockAutofacScope = new Mock<ILifetimeScope>(MockBehavior.Strict);
            var testInstance = new MyService();

            mockAutofacScope.Setup(s => s.Dispose()).Verifiable();

            var scope = new AutofacServiceScope("mytag", mockAutofacScope.Object);

            scope.Dispose();

            mockAutofacScope.Verify(s => s.Dispose(), Times.Once());
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
