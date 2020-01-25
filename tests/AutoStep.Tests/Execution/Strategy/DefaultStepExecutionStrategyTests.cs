using System;
using System.Linq;
using System.Threading.Tasks;
using AutoStep.Compiler;
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

namespace AutoStep.Tests.Execution.Strategy
{
    public class DefaultStepExecutionStrategyTests : LoggingTestBase
    {
        public DefaultStepExecutionStrategyTests(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }

        [Fact]
        public async ValueTask ExecutesStep()
        {
            var step = new StepReferenceBuilder("I have done something", StepType.Given, StepType.Given, 1, 1).Built;

            var variables = new VariableSet();

            var stepContext = new StepContext(0, new StepCollectionContext(), step, variables);

            var mockScope = new Mock<IServiceScope>().Object;

            var ranStep = false;

            var stepDef = new TestStepDef((scope, ctxt, vars) =>
            {
                ranStep = true;
                ctxt.Should().Be(stepContext);
                vars.Should().Be(variables);
                scope.Should().Be(mockScope);

                return default;
            });

            step.Bind(new StepReferenceBinding(stepDef, null));

            var strategy = new DefaultStepExecutionStrategy();

            await strategy.ExecuteStep(mockScope, stepContext, variables);

            ranStep.Should().BeTrue();
        }

        [Fact]
        public void ThrowsUnboundException()
        {
            var step = new StepReferenceBuilder("I have done something", StepType.Given, StepType.Given, 1, 1).Built;

            var variables = new VariableSet();

            var stepContext = new StepContext(0, new StepCollectionContext(), step, variables);

            var mockScope = new Mock<IServiceScope>().Object;

            var ranStep = false;

            var stepDef = new TestStepDef((scope, ctxt, vars) =>
            {
                ranStep = true;

                return default;
            });

            var strategy = new DefaultStepExecutionStrategy();

            strategy.Awaiting(s => s.ExecuteStep(mockScope, stepContext, variables)).Should().Throw<UnboundStepException>();

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
            builder.RegisterSingleInstance(threadContext);
            var scope = builder.BuildRootScope();

            var ranStepCount = 0;

            var strategy = new DefaultStepExecutionStrategy();

            var stepDef = new TestStepDef(async (scope, ctxt, vars) =>
            {
                ranStepCount++;

                // Execute the same step inside itself to trigger a circular reference.
                await strategy.ExecuteStep(scope, ctxt, vars);
            });

            step.Bind(new StepReferenceBinding(stepDef, null));

            var ex = await strategy.Awaiting(s => s.ExecuteStep(scope, stepContext, variables))
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
            builder.RegisterSingleInstance(threadContext);
            var scope = builder.BuildRootScope();

            var ranStepCount = 0;

            var strategy = new DefaultStepExecutionStrategy();

            var stepDef = new TestStepDef("step", async (scope, ctxt, vars) =>
            {
                ranStepCount++;

                // Execute the same step inside itself to trigger a circular reference.
                await strategy.ExecuteStep(scope, loopbackStepContext, vars);
            });

            var loopBackStepDef = new TestStepDef("loopbackStep", async (scope, ctxt, vars) =>
            {
                // Execute the same step inside itself to trigger a circular reference.
                await strategy.ExecuteStep(scope, stepContext, vars);
            });

            step.Bind(new StepReferenceBinding(stepDef, null));
            loopbackStep.Bind(new StepReferenceBinding(loopBackStepDef, null));

            var ex = await strategy.Awaiting(s => s.ExecuteStep(scope, stepContext, variables))
                             .Should().ThrowAsync<CircularStepReferenceException>();

            ex.Which.StepDefinition.Should().Be(stepDef);
            ex.Which.StepExecutionStack.Should().HaveCount(2);
            ex.Which.StepExecutionStack.Should().ContainInOrder(new[] { loopbackStep, step });

            ranStepCount.Should().Be(1);
        }

        private class TestStepDef : StepDefinition
        {
            private readonly Func<IServiceScope, StepContext, VariableSet, ValueTask> callback;

            public TestStepDef(Func<IServiceScope, StepContext, VariableSet, ValueTask> callback)
                : base(TestStepDefinitionSource.Blank, StepType.Given, "something")
            {
                this.callback = callback;
            }

            public TestStepDef(string name, Func<IServiceScope, StepContext, VariableSet, ValueTask> callback)
                : base(TestStepDefinitionSource.Blank, StepType.Given, name)
            {
                this.callback = callback;
            }

            public override async ValueTask ExecuteStepAsync(IServiceScope stepScope, StepContext context, VariableSet variables)
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
