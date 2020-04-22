using System;
using System.Threading;
using System.Threading.Tasks;
using AutoStep.Definitions;
using AutoStep.Definitions.Interaction;
using AutoStep.Execution.Binding;
using AutoStep.Execution.Contexts;
using AutoStep.Execution.Dependency;
using FluentAssertions;
using Moq;
using Xunit;

namespace AutoStep.Tests.Definition.Interaction
{
    public class DelegateInteractionMethodTests
    {
        [Fact]
        public void InvokeAsyncNullScope_Throws()
        {
            Action test = () => { };

            var method = new DelegateInteractionMethod("method", test);

            method.Invoking(async m => await m.InvokeAsync(null!, new MethodContext(), CancellationToken.None))
                  .Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void InvokeAsyncNullContext_Throws()
        {
            Action test = () => { };

            var method = new DelegateInteractionMethod("method", test);

            method.Invoking(async m => await m.InvokeAsync(new Mock<IAutoStepServiceScope>().Object, null!, CancellationToken.None))
                  .Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public async Task CanInvokeTaskMethod()
        {
            var invoked = 0;

            Func<Task> action = async () => { await Task.Delay(1); invoked++; };

            var methodInstance = new DelegateInteractionMethod("method", action);

            var scope = new Mock<IAutoStepServiceScope>();

            await methodInstance.InvokeAsync(scope.Object, new MethodContext(), CancellationToken.None);

            invoked.Should().Be(1);
        }

        [Fact]
        public async Task CanInvokeValueTaskMethod()
        {
            var invoked = 0;

            Func<ValueTask> action = async () => { await Task.Delay(1); invoked++; };

            var methodInstance = new DelegateInteractionMethod("method", action);

            var scope = new Mock<IAutoStepServiceScope>();

            await methodInstance.InvokeAsync(scope.Object, new MethodContext(), CancellationToken.None);

            invoked.Should().Be(1);
        }

        [Fact]
        public async Task ArgumentBinding_NullValueBecomesDefaultValue()
        {
            int? invokedValue = null;

            Action<int> action = val => invokedValue = val;

            var methodInstance = new DelegateInteractionMethod("method", action);

            await methodInstance.InvokeAsync(GetScopeWithRegistry(), new MethodContext(new object?[] { null }), CancellationToken.None);

            invokedValue.Should().Be(0);
        }

        [Fact]
        public async Task ArgumentBinding_NullValuePreservedForReferenceType()
        {
            string invokedValue = "test";

            Action<string> action = val => invokedValue = val;

            var methodInstance = new DelegateInteractionMethod("method", action);

            await methodInstance.InvokeAsync(GetScopeWithRegistry(), new MethodContext(new object?[] { null }), CancellationToken.None);

            invokedValue.Should().BeNull();
        }

        [Fact]
        public async Task ArgumentBinding_StringConvertBinding()
        {
            int value = 0;

            Action<int> action = val => value = val;

            var methodInstance = new DelegateInteractionMethod("method", action);

            await methodInstance.InvokeAsync(GetScopeWithRegistry(), new MethodContext(new object[] { "123" }), CancellationToken.None);

            value.Should().Be(123);
        }

        [Fact]
        public void ArgumentBinding_FailingStringConvertBinding()
        {
            int value = 0;

            Action<int> action = val => value = val;

            var methodInstance = new DelegateInteractionMethod("method", action);

            methodInstance.Invoking(async m => await m.InvokeAsync(GetScopeWithFailingArgumentBinder<int>(), new MethodContext(new object[] { "abc" }), CancellationToken.None))
                          .Should().Throw<ArgumentBindingException>();
        }

        [Fact]
        public async Task ArgumentBinding_DerivedAssignmentBinding()
        {
            BaseClass? value = null;

            Action<BaseClass> action = val => value = val;

            var methodInstance = new DelegateInteractionMethod("method", action);

            var derivedClass = new DerivedClass();

            await methodInstance.InvokeAsync(GetScopeWithRegistry(), new MethodContext(new object[] { derivedClass }), CancellationToken.None);

            value.Should().Be(derivedClass);
        }

        [Fact]
        public async Task ArgumentBinding_CanConvertNumericTypes()
        {
            double value = -1;

            Action<double> action = val => value = val;

            var methodInstance = new DelegateInteractionMethod("method", action);

            // Pass in a decimal.
            await methodInstance.InvokeAsync(GetScopeWithRegistry(), new MethodContext(new object[] { 128.5M }), CancellationToken.None);

            value.Should().Be(128.5);
        }

        [Fact]
        public void ArgumentBinding_CannotConvertIncompatibleTypes()
        {
            DateTime value = DateTime.MinValue;

            Action<DateTime> action = val => value = val;

            var methodInstance = new DelegateInteractionMethod("method", action);

            methodInstance.Invoking(async m => await m.InvokeAsync(GetScopeWithRegistry(), new MethodContext(new object[] { -24 }), CancellationToken.None))
                          .Should().Throw<InvalidCastException>();
        }

        [Fact]
        public void ArgumentBinding_CannotCastIncompatibleTypes()
        {
            string? value = null;

            Action<string> action = val => value = val;

            var methodInstance = new DelegateInteractionMethod("method", action);

            methodInstance.Invoking(async m => await m.InvokeAsync(GetScopeWithRegistry(), new MethodContext(new object[] { new DerivedClass() }), CancellationToken.None))
                          .Should().Throw<InvalidCastException>();
        }

        [Fact]
        public async Task ArgumentBinding_CanBindWithSpecialArguments()
        {
            string? value = null;
            MethodContext? passedCtxt = null;
            IServiceProvider? passedScope = null;
            CancellationToken cancelToken = default;

            Action<MethodContext, string, IServiceProvider, CancellationToken> action = (ctxt, val, scope, cancel) =>
            {
                passedCtxt = ctxt;
                value = val;
                passedScope = scope;
                cancelToken = cancel;
            };

            var methodInstance = new DelegateInteractionMethod("method", action);
            var scope = GetScopeWithRegistry();
            var ctxt = new MethodContext(new object[] { "test" });

            var cancelSource = new CancellationTokenSource();

            await methodInstance.InvokeAsync(scope, ctxt, cancelSource.Token);

            value.Should().Be("test");
            passedCtxt.Should().Be(ctxt);
            passedScope.Should().Be(scope);
            cancelToken.Should().Be(cancelSource.Token);
        }

        [Fact]
        public void ArgumentBinding_SpecialArgumentsExcludedFromCount()
        {

            Action<MethodContext, string, IAutoStepServiceScope, CancellationToken> action = (ctxt, val, scope, cancel) =>
            {
            };

            var methodInstance = new DelegateInteractionMethod("method", action);

            methodInstance.ArgumentCount.Should().Be(1);
        }

        [Fact]
        public async Task ExceptionInInteractionMethodIsUnwrapped()
        {
            Func<Task> action = async () => { await Task.Delay(1); throw new InvalidOperationException(); };

            var methodInstance = new DelegateInteractionMethod("method", action);

            var scope = new Mock<IAutoStepServiceScope>();

            Func<Task> act = async () => await methodInstance.InvokeAsync(scope.Object, new MethodContext(), CancellationToken.None);

            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        private class BaseClass { }

        private class DerivedClass : BaseClass { }

        private IAutoStepServiceScope GetScopeWithRegistry()
        {
            var scope = new Mock<IAutoStepServiceScope>();
            scope.Setup(x => x.GetService(typeof(ArgumentBinderRegistry))).Returns(new ArgumentBinderRegistry());

            return scope.Object;
        }

        private IAutoStepServiceScope GetScopeWithFailingArgumentBinder<TBind>()
        {
            var scope = new Mock<IAutoStepServiceScope>();
            var binder = new ArgumentBinderRegistry();
            binder.RegisterArgumentBinder<FailingBinder>(typeof(TBind));
            scope.Setup(x => x.GetService(typeof(FailingBinder))).Returns(new FailingBinder());
            scope.Setup(x => x.GetService(typeof(ArgumentBinderRegistry))).Returns(binder);

            return scope.Object;
        }

        private class FailingBinder : IArgumentBinder
        {
            public object Bind(string textValue, Type destinationType)
            {
                throw new NotImplementedException();
            }
        }
    }
}
