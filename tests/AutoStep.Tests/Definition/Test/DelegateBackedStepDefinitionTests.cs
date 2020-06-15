using System;
using System.Threading.Tasks;
using AutoStep.Definitions;
using AutoStep.Elements.Parts;
using AutoStep.Elements.StepTokens;
using AutoStep.Execution;
using AutoStep.Execution.Binding;
using AutoStep.Execution.Contexts;
using AutoStep.Execution.Dependency;
using AutoStep.Tests.Builders;
using FluentAssertions;
using Moq;
using Xunit;
using AutoStep.Elements.Test;
using AutoStep.Language.Test;
using AutoStep.Definitions.Test;
using System.Threading;
using Autofac.Core;
using Autofac;

namespace AutoStep.Tests.Definition
{
    public class DelegateBackedStepDefinitionTests
    {
        [Fact]
        public void DelegateDefinitionSameForSameCallback()
        {
            var source = new Mock<IStepDefinitionSource>();

            Action<ILifetimeScope> callback = sc => { };

            var delDefinition1 = new DelegateBackedStepDefinition(source.Object, callback.Target!, callback.Method, StepType.Given, "I test");

            var delDefinition2 = new DelegateBackedStepDefinition(source.Object, callback.Target!, callback.Method, StepType.Given, "I test");

            delDefinition1.IsSameDefinition(delDefinition2).Should().BeTrue();
        }

        [Fact]
        public void DelegateDefinitionDifferentForDifferentCallback()
        {
            var source = new Mock<IStepDefinitionSource>();

            Action<ILifetimeScope> callback = sc => { };
            Action<ILifetimeScope> callback2 = sc => { };

            var delDefinition1 = new DelegateBackedStepDefinition(source.Object, callback.Target!, callback.Method, StepType.Given, "I test");

            var delDefinition2 = new DelegateBackedStepDefinition(source.Object, callback2.Target!, callback2.Method, StepType.Given, "I test");

            delDefinition1.IsSameDefinition(delDefinition2).Should().BeFalse();
        }

        [Fact]
        public async Task CanInvokeDelegateDefinition()
        {
            var source = new Mock<IStepDefinitionSource>();

            var builder = new ContainerBuilder();
            builder.RegisterInstance(new ArgumentBinderRegistry());

            var mockScope = builder.Build();

            var callbackInvoked = false;

            Action<ILifetimeScope> callback = sc =>
            {
                callbackInvoked = true;
            };

            var delDefinition = new DelegateBackedStepDefinition(source.Object, callback.Target!, callback.Method, StepType.Given, "I test");

            var stepRefInfo = new StepReferenceElement();
            stepRefInfo.FreezeTokens();
            stepRefInfo.Bind(new StepReferenceBinding(delDefinition, null, null));

            await delDefinition.ExecuteStepAsync(mockScope, new StepContext(0, null, stepRefInfo, VariableSet.Blank), VariableSet.Blank, CancellationToken.None);

            callbackInvoked.Should().BeTrue();
        }

        [Fact]
        public async Task CanInvokeDelegateDefinitionWithCancellationToken()
        {
            var source = new Mock<IStepDefinitionSource>();
            
            var builder = new ContainerBuilder();
            builder.RegisterInstance(new ArgumentBinderRegistry());
            var mockScope = builder.Build();

            CancellationToken foundCancelToken = default;

            Action<CancellationToken> callback = cancel =>
            {
                foundCancelToken = cancel;
            };

            var delDefinition = new DelegateBackedStepDefinition(source.Object, callback.Target!, callback.Method, StepType.Given, "I test");

            var stepRefInfo = new StepReferenceElement();
            stepRefInfo.FreezeTokens();
            stepRefInfo.Bind(new StepReferenceBinding(delDefinition, null, null));

            var cancellationSource = new CancellationTokenSource();

            await delDefinition.ExecuteStepAsync(mockScope, new StepContext(0, null, stepRefInfo, VariableSet.Blank), VariableSet.Blank, cancellationSource.Token);

            foundCancelToken.Should().Be(cancellationSource.Token);
        }

        [Fact]
        public async Task CanInvokeAsyncTaskBackedDelegateDefinition()
        {
            var source = new Mock<IStepDefinitionSource>();

            var builder = new ContainerBuilder();
            builder.RegisterInstance(new ArgumentBinderRegistry());
            var mockScope = builder.Build();

            var callbackInvoked = false;

            Func<ILifetimeScope, Task> callback = async sc =>
            {
                await Task.Delay(10);
                callbackInvoked = true;
            };

            var delDefinition = new DelegateBackedStepDefinition(source.Object, callback.Target!, callback.Method, StepType.Given, "I test");

            var stepRefInfo = new StepReferenceElement();
            stepRefInfo.FreezeTokens();
            stepRefInfo.Bind(new StepReferenceBinding(delDefinition, null, null));

            await delDefinition.ExecuteStepAsync(mockScope, new StepContext(0, null, stepRefInfo, VariableSet.Blank), VariableSet.Blank, CancellationToken.None);

            callbackInvoked.Should().BeTrue();
        }

        [Fact]
        public async Task CanInvokeAsyncValueTaskDelegateDefinition()
        {
            var source = new Mock<IStepDefinitionSource>();

            var builder = new ContainerBuilder();
            builder.RegisterInstance(new ArgumentBinderRegistry());
            var mockScope = builder.Build();

            var callbackInvoked = false;

            Func<ILifetimeScope, ValueTask> callback = sc =>
            {
                callbackInvoked = true;
                return default;
            };

            var delDefinition = new DelegateBackedStepDefinition(source.Object, callback.Target!, callback.Method, StepType.Given, "I test");

            var stepRefInfo = new StepReferenceElement();
            stepRefInfo.FreezeTokens();
            stepRefInfo.Bind(new StepReferenceBinding(delDefinition, null, null));

            await delDefinition.ExecuteStepAsync(mockScope, new StepContext(0, null, stepRefInfo, VariableSet.Blank), VariableSet.Blank, CancellationToken.None);

            callbackInvoked.Should().BeTrue();
        }

        [Fact]
        public async Task CanInvokeDelegateDefinitionBindArgument()
        {
            var source = new Mock<IStepDefinitionSource>();
            var scopeBuilder = new ContainerBuilder();
            scopeBuilder.RegisterInstance(new ArgumentBinderRegistry());

            string? argValue = null;

            Action<string> callback = arg1 =>
            {
                argValue = arg1;
            };

            var step = new StepReferenceBuilder("I test", StepType.Given, StepType.Given, 1, 1)
                                .Text("I").Text("test").Built;

            step.FreezeTokens();

            var delDefinition = new DelegateBackedStepDefinition(source.Object, callback.Target!, callback.Method, StepType.Given, "I {arg1}");

            step.Bind(new StepReferenceBinding(delDefinition, new[] {
                new ArgumentBinding(new ArgumentPart("{arg1}", "arg1", ArgumentType.Text),
                                    new StepReferenceMatchResult(4, true, new ReadOnlySpan<StepToken>(), step.TokenSpan.Slice(1)))
            }, null));

            await delDefinition.ExecuteStepAsync(scopeBuilder.Build(), new StepContext(0, null, step, VariableSet.Blank), VariableSet.Blank, CancellationToken.None);

            argValue.Should().Be("test");
        }

        [Fact]
        public void ConversionExceptionsResultInArgumentBindingException()
        {
            var source = new Mock<IStepDefinitionSource>();
            var scopeBuilder = new ContainerBuilder();
            scopeBuilder.RegisterInstance(new ArgumentBinderRegistry());

            Action<int> callback = arg1 =>
            {
            };

            var step = new StepReferenceBuilder("I test", StepType.Given, StepType.Given, 1, 1)
                                .Text("I").Text("test").Built;

            step.FreezeTokens();

            var delDefinition = new DelegateBackedStepDefinition(source.Object, callback.Target!, callback.Method, StepType.Given, "I {arg1}");

            step.Bind(new StepReferenceBinding(delDefinition, new[] {
                new ArgumentBinding(new ArgumentPart("{arg1}", "arg1", ArgumentType.Text),
                                    new StepReferenceMatchResult(4, true, new ReadOnlySpan<StepToken>(), step.TokenSpan.Slice(1)))
            }, null));

            delDefinition.Invoking(x => x.ExecuteStepAsync(scopeBuilder.Build(), new StepContext(0, null, step, VariableSet.Blank), VariableSet.Blank, CancellationToken.None))
                         .Should()
                         .Throw<ArgumentBindingException>()
                         .Where(e => e.TextValue == "test")
                         .Where(e => e.ExpectedType == typeof(int));
        }

        [Fact]
        public async Task CanInvokeDelegateDefinitionNoArguments()
        {
            var source = new Mock<IStepDefinitionSource>();
            var scopeBuilder = new ContainerBuilder();
            scopeBuilder.RegisterInstance(new ArgumentBinderRegistry());

            var callbackInvoked = false;

            Action callback = () =>
            {
                callbackInvoked = true;
            };

            var delDefinition = new DelegateBackedStepDefinition(source.Object, callback.Target!, callback.Method, StepType.Given, "I test");

            var stepRefInfo = new StepReferenceElement();
            stepRefInfo.FreezeTokens();
            stepRefInfo.Bind(new StepReferenceBinding(delDefinition, null, null));

            await delDefinition.ExecuteStepAsync(scopeBuilder.Build(), new StepContext(0, null, stepRefInfo, VariableSet.Blank), VariableSet.Blank, CancellationToken.None);

            callbackInvoked.Should().BeTrue();
        }

        [Fact]
        public void DelegateExceptionIsUnwrapped()
        {
            var source = new Mock<IStepDefinitionSource>();

            var builder = new ContainerBuilder();
            builder.RegisterInstance(new ArgumentBinderRegistry());
            var mockScope = builder.Build();

            Action<ILifetimeScope> callback = sc =>
            {
                throw new InvalidOperationException();
            };

            var delDefinition = new DelegateBackedStepDefinition(source.Object, callback.Target!, callback.Method, StepType.Given, "I test");

            var stepRefInfo = new StepReferenceElement();
            stepRefInfo.FreezeTokens();
            stepRefInfo.Bind(new StepReferenceBinding(delDefinition, null, null));

            delDefinition.Invoking(d => d.ExecuteStepAsync(mockScope, new StepContext(0, null, stepRefInfo, VariableSet.Blank), VariableSet.Blank, CancellationToken.None))
                         .Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public async Task CanInvokeMethodWithTableBinding()
        {
            var source = new Mock<IStepDefinitionSource>();

            var builder = new ContainerBuilder();
            builder.RegisterInstance(new ArgumentBinderRegistry());
            var mockScope = builder.Build();

            Table? foundTable = default;

            Action<Table> callback = table =>
            {
                foundTable = table;
            };

            var delDefinition = new DelegateBackedStepDefinition(source.Object, callback.Target!, callback.Method, StepType.Given, "I test");

            var stepRefInfo = new StepReferenceElement
            {
                Table = new TableElement()
            };
            stepRefInfo.FreezeTokens();
            stepRefInfo.Bind(new StepReferenceBinding(delDefinition, null, null));

            var cancellationSource = new CancellationTokenSource();

            await delDefinition.ExecuteStepAsync(mockScope, new StepContext(0, null, stepRefInfo, VariableSet.Blank), VariableSet.Blank, cancellationSource.Token);

            foundTable.Should().NotBeNull();
        }
    }
}
