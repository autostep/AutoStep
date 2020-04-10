using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoStep.Elements.Metadata;
using AutoStep.Execution;
using AutoStep.Execution.Contexts;
using AutoStep.Execution.Control;
using AutoStep.Execution.Dependency;
using AutoStep.Execution.Events;
using AutoStep.Execution.Strategy;
using AutoStep.Tests.Builders;
using AutoStep.Tests.Utils;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace AutoStep.Tests.Execution.Strategy
{
    public class DefaultScenarioExecutionStrategyTests : LoggingTestBase
    {
        public DefaultScenarioExecutionStrategyTests(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }

        [Fact]
        public async ValueTask ScenarioWithBackground()
        {
            var feature = new FeatureBuilder("My Feature 1", 1, 1)
                                            .Background(1, 1, b => b.Given("I have done something", 1, 1))
                                            .Scenario("My Scenario 1", 1, 1, cfg => cfg.Given("I have done something else", 1, 1))
                                            .Built;

            await DoTest(feature, feature.Scenarios[0], VariableSet.Blank, false,
                         (feature.Background!, VariableSet.Blank),
                         (feature.Scenarios[0], VariableSet.Blank));
        }

        [Fact]
        public async ValueTask BackgroundErrorStopsScenarioFromRunning()
        {
            var feature = new FeatureBuilder("My Feature 1", 1, 1)
                                            .Background(1, 1, b => b.Given("I have done something", 1, 1))
                                            .Scenario("My Scenario 1", 1, 1, cfg => cfg.Given("I have done something else", 1, 1))
                                            .Built;

            await DoTest(feature, feature.Scenarios[0], VariableSet.Blank, true,
                         (feature.Background!, VariableSet.Blank));
        }

        [Fact]
        public async ValueTask ScenarioWithoutBackground()
        {
            var feature = new FeatureBuilder("My Feature 1", 1, 1)
                                            .Scenario("My Scenario 1", 1, 1, cfg => cfg.Given("I have done something else", 1, 1))
                                            .Built;

            await DoTest(feature, feature.Scenarios[0], VariableSet.Blank, false,
                         (feature.Scenarios[0], VariableSet.Blank));
        }

        [Fact]
        public async ValueTask UsesInstructedScenario()
        {
            var feature = new FeatureBuilder("My Feature 1", 1, 1)
                                            .Scenario("My Scenario 1", 1, 1, cfg => cfg.Given("I have done something else", 1, 1))
                                            .Scenario("My Scenario 2", 1, 1, cfg => cfg.Given("I have done something else", 1, 1))
                                            .Built;

            await DoTest(feature, feature.Scenarios[1], VariableSet.Blank, false,
                         (feature.Scenarios[1], VariableSet.Blank));
        }

        [Fact]
        public async ValueTask VariableSetPassedThrough()
        {
            var feature = new FeatureBuilder("My Feature 1", 1, 1)
                                            .Background(1, 1, b => b.Given("I have done something", 1, 1))
                                            .Scenario("My Scenario 1", 1, 1, cfg => cfg.Given("I have done something else", 1, 1))
                                            .Built;

            var variables = new VariableSet();
            variables.Set("var", "value1");

            await DoTest(feature, feature.Scenarios[0], variables, false,
                         (feature.Background!, variables),
                         (feature.Scenarios[0], variables));
        }

        private async ValueTask DoTest(IFeatureInfo feature, IScenarioInfo scenario, VariableSet variables, bool raiseException, params (IStepCollectionInfo collection, VariableSet variables)[] collections)
        {
            var mockExecutionStateManager = new Mock<IExecutionStateManager>();
            var beforeFeat = 0;
            var afterFeat = 0;
            var eventHandler = new MyEventHandler(ctxt =>
            {
                ctxt.Scenario.Should().Be(scenario);
                beforeFeat++;
            }, c => afterFeat++);

            var eventPipeline = new EventPipeline(new List<IEventHandler> { eventHandler });
            var stepCollectionStrategy = new MyStepCollectionStrategy(raiseException);

            var builder = new AutofacServiceBuilder();

            builder.RegisterInstance(LogFactory);
            builder.RegisterInstance(mockExecutionStateManager.Object);
            builder.RegisterInstance<IEventPipeline>(eventPipeline);
            builder.RegisterInstance<IStepCollectionExecutionStrategy>(stepCollectionStrategy);

            var scope = builder.BuildRootScope();

            var strategy = new DefaultScenarioExecutionStrategy();

            var featureContext = new FeatureContext(feature);

            await strategy.Execute(scope, featureContext, scenario, variables);

            beforeFeat.Should().Be(1);
            afterFeat.Should().Be(1);
            mockExecutionStateManager.Verify(x => x.CheckforHalt(It.IsAny<IAutoStepServiceScope>(), It.IsAny<ScenarioContext>(), TestThreadState.StartingScenario), Times.Once());

            stepCollectionStrategy.AddedCollections.Should().BeEquivalentTo(collections);
        }

        private class MyStepCollectionStrategy : IStepCollectionExecutionStrategy
        {
            private bool raiseException;

            public MyStepCollectionStrategy(bool raiseException)
            {
                this.raiseException = raiseException;
            }

            public List<(IStepCollectionInfo scenario, VariableSet variables)> AddedCollections { get; } = new List<(IStepCollectionInfo, VariableSet)>();

            public ValueTask Execute(IAutoStepServiceScope owningScope, StepCollectionContext owningContext, IStepCollectionInfo stepCollection, VariableSet variables)
            {
                owningScope.Should().NotBeNull();
                owningContext.Should().BeOfType<ScenarioContext>();

                AddedCollections.Add((stepCollection, variables));

                if (raiseException)
                {
                    owningContext.FailException = new NullReferenceException();
                }

                return default;
            }
        }

        private class MyEventHandler : BaseEventHandler
        {
            private readonly Action<ScenarioContext> callBefore;
            private readonly Action<ScenarioContext> callAfter;
            private readonly Action<Exception>? exception;

            public MyEventHandler(Action<ScenarioContext> callBefore, Action<ScenarioContext> callAfter, Action<Exception>? exception = null)
            {
                this.callBefore = callBefore;
                this.callAfter = callAfter;
                this.exception = exception;
            }

            public MyEventHandler(Action<Exception> exception)
            {
                this.callBefore = c => { };
                this.callAfter = c => { };
                this.exception = exception;
            }

            public override async ValueTask OnScenario(IServiceProvider scope, ScenarioContext ctxt, Func<IServiceProvider, ScenarioContext, ValueTask> next)
            {
                callBefore(ctxt);

                try
                {
                    await next(scope, ctxt);
                }
                catch (Exception ex)
                {
                    if (exception is object)
                    {
                        exception(ex);
                    }
                    else
                    {
                        throw;
                    }
                }

                callAfter(ctxt);
            }
        }
    }
}
