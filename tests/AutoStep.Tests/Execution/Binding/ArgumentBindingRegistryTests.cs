using System;
using Autofac;
using AutoStep.Execution.Binding;
using AutoStep.Execution.Dependency;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace AutoStep.Tests.Execution.Binding
{
    public class ArgumentBindingRegistryTests
    {
        [Fact]
        public void CanRegisterABinder()
        {
            var binderRegistry = new ArgumentBinderRegistry();

            var builder = new ContainerBuilder();

            var binder = new MyCustomBinder<DateTime>();

            builder.RegisterInstance(binder);

            binderRegistry.RegisterArgumentBinder<MyCustomBinder<DateTime>>(typeof(DateTime));

            var foundBinder = binderRegistry.GetBinderForType(builder.Build(), typeof(DateTime));

            foundBinder.Should().Be(binder);
        }

        [Fact]
        public void UnregisteredBinderReturnsDefault()
        {
            var binderRegistry = new ArgumentBinderRegistry();

            binderRegistry.RegisterArgumentBinder<MyCustomBinder<DateTime>>(typeof(DateTime));

            var foundBinder = binderRegistry.GetBinderForType(new ContainerBuilder().Build(), typeof(decimal));

            foundBinder.Should().BeOfType(typeof(DefaultArgumentBinder));
        }

        [Fact]
        public void BindersResolvedFromScopeEachTime()
        {
            var binderRegistry = new ArgumentBinderRegistry();

            var builder = new ContainerBuilder();

            var activatedCount = 0;

            builder.RegisterType<MyCustomBinder<DateTime>>().InstancePerDependency().OnActivating(args => activatedCount++);

            var container = builder.Build();

            binderRegistry.RegisterArgumentBinder<MyCustomBinder<DateTime>>(typeof(DateTime));

            binderRegistry.GetBinderForType(container, typeof(DateTime));
            binderRegistry.GetBinderForType(container, typeof(DateTime));

            activatedCount.Should().Be(2);
        }

        [Fact]
        public void NullArgumentOnScopeThrows()
        {
            var binderRegistry = new ArgumentBinderRegistry();

            Action act = () => binderRegistry.GetBinderForType(null!, typeof(decimal));

            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void NullArgumentOnParameterTypeThrows()
        {
            var binderRegistry = new ArgumentBinderRegistry();

            Action act = () => binderRegistry.GetBinderForType(new Mock<ILifetimeScope>().Object, null!);

            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void NullArgumentTypeOnRegistrationThrows()
        {
            var binderRegistry = new ArgumentBinderRegistry();

            Action act = () => binderRegistry.RegisterArgumentBinder<MyCustomBinder<decimal>>(null!);

            act.Should().Throw<ArgumentNullException>();
        }

        private class MyCustomBinder<TTypeFor> : IArgumentBinder
        {
            public object Bind(string textValue, Type destinationType)
            {
                throw new NotImplementedException();
            }
        }
    }
}
