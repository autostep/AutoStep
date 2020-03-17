using System;
using System.Threading.Tasks;
using AutoStep.Definitions;
using AutoStep.Definitions.Interaction;
using AutoStep.Execution.Binding;
using AutoStep.Execution.Dependency;
using AutoStep.Execution.Interaction;
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

            method.Invoking(async m => await m.InvokeAsync(null, new MethodContext(), Array.Empty<object>()))
                  .Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void InvokeAsyncNullContext_Throws()
        {
            Action test = () => { };

            var method = new DelegateInteractionMethod("method", test);

            method.Invoking(async m => await m.InvokeAsync(new Mock<IServiceScope>().Object, null, Array.Empty<object>()))
                  .Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void InvokeAsyncNullArguments_Throws()
        {
            Action test = () => { };

            var method = new DelegateInteractionMethod("method", test);

            method.Invoking(async m => await m.InvokeAsync(new Mock<IServiceScope>().Object, new MethodContext(), null))
                  .Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public async Task CanInvokeTaskMethod()
        {
            var invoked = 0;

            Func<Task> action = async () => { await Task.Delay(1); invoked++; };

            var methodInstance = new DelegateInteractionMethod("method", action);

            var scope = new Mock<IServiceScope>();

            await methodInstance.InvokeAsync(scope.Object, new MethodContext(), Array.Empty<object>());

            invoked.Should().Be(1);
        }

        [Fact]
        public async Task CanInvokeValueTaskMethod()
        {
            var invoked = 0;

            Func<ValueTask> action = async () => { await Task.Delay(1); invoked++; };

            var methodInstance = new DelegateInteractionMethod("method", action);

            var scope = new Mock<IServiceScope>();

            await methodInstance.InvokeAsync(scope.Object, new MethodContext(), Array.Empty<object>());

            invoked.Should().Be(1);
        }

        [Fact]
        public async Task ArgumentBinding_NullValueBecomesDefaultValue()
        {
            int? invokedValue = null;

            Action<int> action = val => invokedValue = val;

            var methodInstance = new DelegateInteractionMethod("method", action);

            await methodInstance.InvokeAsync(GetScopeWithRegistry(), new MethodContext(), new object[] { null });

            invokedValue.Should().Be(0);
        }

        [Fact]
        public async Task ArgumentBinding_NullValuePreservedForReferenceType()
        {
            string invokedValue = "test";

            Action<string> action = val => invokedValue = val;

            var methodInstance = new DelegateInteractionMethod("method", action);

            await methodInstance.InvokeAsync(GetScopeWithRegistry(), new MethodContext(), new object[] { null });

            invokedValue.Should().BeNull();
        }

        [Fact]
        public async Task ArgumentBinding_StringConvertBinding()
        {
            int value = 0;

            Action<int> action = val => value = val;

            var methodInstance = new DelegateInteractionMethod("method", action);

            await methodInstance.InvokeAsync(GetScopeWithRegistry(), new MethodContext(), new object[] { "123" });

            value.Should().Be(123);
        }

        [Fact]
        public void ArgumentBinding_FailingStringConvertBinding()
        {
            int value = 0;

            Action<int> action = val => value = val;

            var methodInstance = new DelegateInteractionMethod("method", action);

            methodInstance.Invoking(async m => await m.InvokeAsync(GetScopeWithFailingArgumentBinder<int>(), new MethodContext(), new object[] { "abc" }))
                          .Should().Throw<ArgumentBindingException>();
        }

        [Fact]
        public async Task ArgumentBinding_DerivedAssignmentBinding()
        {
            BaseClass value = null;

            Action<BaseClass> action = val => value = val;

            var methodInstance = new DelegateInteractionMethod("method", action);

            var derivedClass = new DerivedClass();

            await methodInstance.InvokeAsync(GetScopeWithRegistry(), new MethodContext(), new object[] { derivedClass });

            value.Should().Be(derivedClass);
        }

        [Fact]
        public async Task ArgumentBinding_CanConvertNumericTypes()
        {
            double value = -1;

            Action<double> action = val => value = val;

            var methodInstance = new DelegateInteractionMethod("method", action);

            // Pass in a decimal.
            await methodInstance.InvokeAsync(GetScopeWithRegistry(), new MethodContext(), new object[] { 128.5M });

            value.Should().Be(128.5);
        }

        [Fact]
        public void ArgumentBinding_CannotConvertIncompatibleTypes()
        {
            DateTime value = DateTime.MinValue;

            Action<DateTime> action = val => value = val;

            var methodInstance = new DelegateInteractionMethod("method", action);

            methodInstance.Invoking(async m => await m.InvokeAsync(GetScopeWithRegistry(), new MethodContext(), new object[] { -24 }))
                          .Should().Throw<InvalidCastException>();
        }

        [Fact]
        public void ArgumentBinding_CannotCastIncompatibleTypes()
        {
            string value = null;

            Action<string> action = val => value = val;

            var methodInstance = new DelegateInteractionMethod("method", action);

            methodInstance.Invoking(async m => await m.InvokeAsync(GetScopeWithRegistry(), new MethodContext(), new object[] { new DerivedClass() }))
                          .Should().Throw<InvalidCastException>();
        }

        [Fact]
        public async Task ArgumentBinding_CanBindWithSpecialArguments()
        {
            string value = null;
            MethodContext passedCtxt = null;
            IServiceScope passedScope = null;

            Action<MethodContext, string, IServiceScope> action = (ctxt, val, scope) =>
            {
                passedCtxt = ctxt;
                value = val;
                passedScope = scope;

            };

            var methodInstance = new DelegateInteractionMethod("method", action);
            var scope = GetScopeWithRegistry();
            var ctxt = new MethodContext();

            await methodInstance.InvokeAsync(scope, ctxt, new object[] { "test" });

            value.Should().Be("test");
            passedCtxt.Should().Be(ctxt);
            passedScope.Should().Be(scope);
        }

        [Fact]
        public void ArgumentBinding_SpecialArgumentsExcludedFromCount()
        {
            string value = null;
            MethodContext passedCtxt = null;
            IServiceScope passedScope = null;

            Action<MethodContext, string, IServiceScope> action = (ctxt, val, scope) =>
            {
                passedCtxt = ctxt;
                value = val;
                passedScope = scope;
            };

            var methodInstance = new DelegateInteractionMethod("method", action);

            methodInstance.ArgumentCount.Should().Be(1);
        }

        private class BaseClass { }

        private class DerivedClass : BaseClass { }

        private IServiceScope GetScopeWithRegistry()
        {
            var scope = new Mock<IServiceScope>();
            scope.Setup(x => x.Resolve<ArgumentBinderRegistry>()).Returns(new ArgumentBinderRegistry());

            return scope.Object;
        }

        private IServiceScope GetScopeWithFailingArgumentBinder<TBind>()
        {
            var scope = new Mock<IServiceScope>();
            var binder = new ArgumentBinderRegistry();
            binder.RegisterArgumentBinder<FailingBinder>(typeof(TBind));
            scope.Setup(x => x.Resolve(typeof(FailingBinder))).Returns(new FailingBinder());
            scope.Setup(x => x.Resolve<ArgumentBinderRegistry>()).Returns(binder);

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
