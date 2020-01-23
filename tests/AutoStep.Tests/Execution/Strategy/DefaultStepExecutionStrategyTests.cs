using System;
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
        public async Task ExecutesStep()
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
            });

            step.Bind(new StepReferenceBinding(stepDef, null));

            var strategy = new DefaultStepExecutionStrategy();

            await strategy.ExecuteStep(mockScope, stepContext, variables);
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
            });

            var strategy = new DefaultStepExecutionStrategy();

            strategy.Invoking(s => s.ExecuteStep(mockScope, stepContext, variables)).Should().Throw<UnboundStepException>();

            ranStep.Should().BeFalse();
        }

        private class TestStepDef : StepDefinition
        {
            private readonly Action<IServiceScope, StepContext, VariableSet> callback;

            public TestStepDef(Action<IServiceScope, StepContext, VariableSet> callback)
                : base(TestStepDefinitionSource.Blank, StepType.Given, "something")
            {
                this.callback = callback;
            }

            public override Task ExecuteStepAsync(IServiceScope stepScope, StepContext context, VariableSet variables)
            {
                callback(stepScope, context, variables);

                return Task.CompletedTask;
            }

            public override bool IsSameDefinition(StepDefinition def)
            {
                throw new NotImplementedException();
            }
        }
    }
}
