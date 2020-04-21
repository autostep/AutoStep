using System;
using System.Threading.Tasks;
using AutoStep.Definitions;
using AutoStep.Execution;
using AutoStep.Execution.Contexts;
using AutoStep.Execution.Dependency;
using AutoStep.Execution.Strategy;
using AutoStep.Tests.Builders;
using AutoStep.Tests.Utils;
using FluentAssertions;
using Moq;
using Xunit;
using Xunit.Abstractions;
using AutoStep.Language.Test;
using System.Threading;

namespace AutoStep.Tests.Execution.Strategy
{
    public class DefaultStepExecutionStrategyTests : LoggingTestBase
    {
        public DefaultStepExecutionStrategyTests(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }

        [Fact]
        public async Task ExecutesStep()
        {
            var step = new StepReferenceBuilder("I have done something", StepType.Given, StepType.Given, 1, 1).Built;

            var variables = new VariableSet();

            var stepContext = new StepContext(0, new StepCollectionContext(), step, variables);

            var mockScope = new Mock<IAutoStepServiceScope>();

            mockScope.Setup(x => x.GetService(typeof(ThreadContext))).Returns(new ThreadContext(1));

            var ranStep = false;

            var stepDef = new TestStepDef((scope, ctxt, vars) =>
            {
                ranStep = true;
                ctxt.Should().Be(stepContext);
                vars.Should().Be(variables);
                scope.Should().Be(mockScope.Object);

                return default;
            });

            step.Bind(new StepReferenceBinding(stepDef, null, null));

            var strategy = new DefaultStepExecutionStrategy();

            await strategy.ExecuteStepAsync(mockScope.Object, stepContext, variables, CancellationToken.None);

            ranStep.Should().BeTrue();
        }

        [Fact]
        public void ThrowsUnboundException()
        {
            var step = new StepReferenceBuilder("I have done something", StepType.Given, StepType.Given, 1, 1).Built;

            var variables = new VariableSet();

            var stepContext = new StepContext(0, new StepCollectionContext(), step, variables);

            var mockScope = new Mock<IAutoStepServiceScope>().Object;

            var ranStep = false;

            var stepDef = new TestStepDef((scope, ctxt, vars) =>
            {
                ranStep = true;

                return default;
            });

            var strategy = new DefaultStepExecutionStrategy();

            strategy.Awaiting(s => s.ExecuteStepAsync(mockScope, stepContext, variables, CancellationToken.None)).Should().Throw<UnboundStepException>();

            ranStep.Should().BeFalse();
        }

        [Fact]
        public async Task CircularReferenceDetection()
        {
            var step = new StepReferenceBuilder("I have done something", StepType.Given, StepType.Given, 1, 1).Built;

            var variables = new VariableSet();

            var stepContext = new StepContext(0, new StepCollectionContext(), step, variables);

            var threadContext = new ThreadContext(1);

            var builder = new AutofacServiceBuilder();
            builder.RegisterInstance(threadContext);
            var scope = builder.BuildRootScope();

            var ranStepCount = 0;

            var strategy = new DefaultStepExecutionStrategy();

            var stepDef = new TestStepDef(async (scope, ctxt, vars) =>
            {
                ranStepCount++;

                // Execute the same step inside itself to trigger a circular reference.
                await strategy.ExecuteStepAsync((IAutoStepServiceScope)scope, ctxt, vars, CancellationToken.None);
            });

            step.Bind(new StepReferenceBinding(stepDef, null, null));

            var ex = await strategy.Awaiting(s => s.ExecuteStepAsync(scope, stepContext, variables, CancellationToken.None))
                             .Should().ThrowAsync<CircularStepReferenceException>();

            ex.Which.StepDefinition.Should().Be(stepDef);
            ex.Which.StepExecutionStack.Should().HaveCount(1);
            ex.Which.StepExecutionStack.Should().Contain(step);

            ranStepCount.Should().Be(1);
        }

        [Fact]
        public async Task IndirectCircularReferenceDetection()
        {
            var step = new StepReferenceBuilder("I have done something", StepType.Given, StepType.Given, 1, 1).Built;

            var loopbackStep = new StepReferenceBuilder("I have called done something", StepType.Given, StepType.Given, 1, 1).Built;

            var variables = new VariableSet();

            var stepContext = new StepContext(0, new StepCollectionContext(), step, variables);
            var loopbackStepContext = new StepContext(0, new StepCollectionContext(), loopbackStep, variables);

            var threadContext = new ThreadContext(1);

            var builder = new AutofacServiceBuilder();
            builder.RegisterInstance(threadContext);
            var scope = builder.BuildRootScope();

            var ranStepCount = 0;

            var strategy = new DefaultStepExecutionStrategy();

            var stepDef = new TestStepDef("step", async (scope, ctxt, vars) =>
            {
                ranStepCount++;

                // Execute the same step inside itself to trigger a circular reference.
                await strategy.ExecuteStepAsync((IAutoStepServiceScope)scope, loopbackStepContext, vars, CancellationToken.None);
            });

            var loopBackStepDef = new TestStepDef("loopbackStep", async (scope, ctxt, vars) =>
            {
                // Execute the same step inside itself to trigger a circular reference.
                await strategy.ExecuteStepAsync((IAutoStepServiceScope)scope, stepContext, vars, CancellationToken.None);
            });

            step.Bind(new StepReferenceBinding(stepDef, null, null));
            loopbackStep.Bind(new StepReferenceBinding(loopBackStepDef, null, null));

            var ex = await strategy.Awaiting(s => s.ExecuteStepAsync(scope, stepContext, variables, CancellationToken.None))
                             .Should().ThrowAsync<CircularStepReferenceException>();

            ex.Which.StepDefinition.Should().Be(stepDef);
            ex.Which.StepExecutionStack.Should().HaveCount(2);
            ex.Which.StepExecutionStack.Should().ContainInOrder(new[] { loopbackStep, step });

            ranStepCount.Should().Be(1);
        }

        private class TestStepDef : StepDefinition
        {
            private readonly Func<IServiceProvider, StepContext, VariableSet, ValueTask> callback;

            public TestStepDef(Func<IServiceProvider, StepContext, VariableSet, ValueTask> callback)
                : base(TestStepDefinitionSource.Blank, StepType.Given, "something")
            {
                this.callback = callback;
            }

            public TestStepDef(string name, Func<IServiceProvider, StepContext, VariableSet, ValueTask> callback)
                : base(TestStepDefinitionSource.Blank, StepType.Given, name)
            {
                this.callback = callback;
            }

            public override async ValueTask ExecuteStepAsync(IServiceProvider stepScope, StepContext context, VariableSet variables, CancellationToken cancelToken)
            {
                await callback(stepScope, context, variables);
            }


            public override bool IsSameDefinition(StepDefinition def)
            {
                return def == this;
            }
        }
    }
}
