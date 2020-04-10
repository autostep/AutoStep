using System;
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

            var scope = new Mock<IAutoStepServiceScope>(MockBehavior.Strict);

            var binder = new MyCustomBinder<DateTime>();
            scope.Setup(x => x.GetService(typeof(MyCustomBinder<DateTime>))).Returns(binder);

            binderRegistry.RegisterArgumentBinder<MyCustomBinder<DateTime>>(typeof(DateTime));

            var foundBinder = binderRegistry.GetBinderForType(scope.Object, typeof(DateTime));

            foundBinder.Should().Be(binder);
        }

        [Fact]
        public void UnregisteredBinderReturnsDefault()
        {
            var binderRegistry = new ArgumentBinderRegistry();

            var scope = new Mock<IAutoStepServiceScope>(MockBehavior.Strict);

            binderRegistry.RegisterArgumentBinder<MyCustomBinder<DateTime>>(typeof(DateTime));

            var foundBinder = binderRegistry.GetBinderForType(scope.Object, typeof(decimal));

            foundBinder.Should().BeOfType(typeof(DefaultArgumentBinder));
        }

        [Fact]
        public void BindersResolvedFromScopeEachTime()
        {
            var binderRegistry = new ArgumentBinderRegistry();

            var scope = new Mock<IAutoStepServiceScope>(MockBehavior.Strict);

            var binder = new MyCustomBinder<DateTime>();
            scope.Setup(x => x.GetService(typeof(MyCustomBinder<DateTime>))).Returns(binder).Verifiable();

            binderRegistry.RegisterArgumentBinder<MyCustomBinder<DateTime>>(typeof(DateTime));

            binderRegistry.GetBinderForType(scope.Object, typeof(DateTime));
            binderRegistry.GetBinderForType(scope.Object, typeof(DateTime));

            scope.Verify(x => x.GetService(typeof(MyCustomBinder<DateTime>)), Times.Exactly(2));
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

            Action act = () => binderRegistry.GetBinderForType(new Mock<IAutoStepServiceScope>().Object, null!);

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
