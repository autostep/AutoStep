using System;
using System.Collections.Generic;
using System.Threading;
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
    public class DefaultFeatureExecutionStrategyTests : LoggingTestBase
    {
        public DefaultFeatureExecutionStrategyTests(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }

        [Fact]
        public async ValueTask SingleScenarioTest()
        {
            var feature = new FeatureBuilder("My Feature 1", 1, 1)
                                            .Scenario("My Scenario 1", 1, 1)
                                            .Built;

            await DoTest(feature, (feature.Scenarios[0], VariableSet.Blank));
        }

        [Fact]
        public async ValueTask MultiScenarioTest()
        {
            var feature = new FeatureBuilder("My Feature 1", 1, 1)
                                            .Scenario("My Scenario 1", 1, 1)
                                            .Scenario("My Scenario 2", 2, 1)
                                            .Built;

            await DoTest(feature,
                (feature.Scenarios[0], VariableSet.Blank),
                (feature.Scenarios[1], VariableSet.Blank)
            );
        }

        [Fact]
        public async ValueTask ExpandedScenarioOutlineTest()
        {
            var feature = new FeatureBuilder("My Feature 1", 1, 1)
                .ScenarioOutline("My Scenario Outline", 1, 1, cfg => cfg
                    .Examples(1, 1, cfg => cfg
                        .Table(1, 1, t => t
                            .Headers(9, 21,
                                ("variable1", 23, 31),
                                ("variable2", 37, 45)
                            )
                            .Row(10, 21,
                                ("something1", 23, 32, cfg => cfg.Text("something").Int("1")),
                                ("something2", 38, 50, cfg => cfg.Text("something").Int("2"))
                            )
                            .Row(10, 21,
                                ("something3", 23, 32, cfg => cfg.Text("something").Int("3")),
                                ("something4", 38, 50, cfg => cfg.Text("something").Int("4"))
                            )
                        )
                    )
                ).Built;

            var firstVariableSet = new VariableSet();
            firstVariableSet.Set("variable1", "something1");
            firstVariableSet.Set("variable2", "something2");

            var secondVariableSet = new VariableSet();
            secondVariableSet.Set("variable1", "something3");
            secondVariableSet.Set("variable2", "something4");

            await DoTest(feature,
                (feature.Scenarios[0], firstVariableSet),
                (feature.Scenarios[0], secondVariableSet)
            );
        }

        [Fact]
        public async ValueTask ExpandedScenarioOutlineMultipleExamplesTest()
        {
            var feature = new FeatureBuilder("My Feature 1", 1, 1)
                .ScenarioOutline("My Scenario Outline", 1, 1, cfg => cfg
                    .Examples(1, 1, cfg => cfg
                        .Table(1, 1, t => t
                            .Headers(9, 21,
                                ("variable1", 23, 31),
                                ("variable2", 37, 45)
                            )
                            .Row(10, 21,
                                ("something1", 23, 32, cfg => cfg.Text("something").Int("1")),
                                ("something2", 38, 50, cfg => cfg.Text("something").Int("2"))
                            )
                        )
                    )
                    .Examples(1, 1, cfg => cfg
                        .Table(1, 1, t => t
                            .Headers(9, 21,
                                ("variable1", 23, 31),
                                ("variable3", 37, 45)
                            )
                            .Row(10, 21,
                                ("something3", 23, 32, cfg => cfg.Text("something").Int("3")),
                                ("something4", 38, 50, cfg => cfg.Text("something").Int("4"))
                            )
                        )
                    )
                ).Built;

            var firstVariableSet = new VariableSet();
            firstVariableSet.Set("variable1", "something1");
            firstVariableSet.Set("variable2", "something2");

            var secondVariableSet = new VariableSet();
            secondVariableSet.Set("variable1", "something3");
            secondVariableSet.Set("variable3", "something4");

            await DoTest(feature,
                (feature.Scenarios[0], firstVariableSet),
                (feature.Scenarios[0], secondVariableSet)
            );
        }

        private async ValueTask DoTest(IFeatureInfo feature, params (IScenarioInfo scenario, VariableSet variables)[] scenarios)
        {
            var threadContext = new ThreadContext(1);
            var mockExecutionStateManager = new Mock<IExecutionStateManager>();
            var beforeFeat = 0;
            var afterFeat = 0;
            var eventHandler = new MyEventHandler(ctxt =>
            {
                ctxt.Feature.Should().Be(feature);
                beforeFeat++;
            }, c => afterFeat++);

            var scenarioStrategy = new MyScenarioStrategy();

            var builder = new AutofacServiceBuilder();

            builder.RegisterInstance(LogFactory);
            builder.RegisterInstance(mockExecutionStateManager.Object);
            builder.RegisterInstance<IScenarioExecutionStrategy>(scenarioStrategy);

            var scope = builder.BuildRootScope();

            var strategy = new DefaultFeatureExecutionStrategy();

            await strategy.ExecuteAsync(scope, threadContext, feature, CancellationToken.None);

            beforeFeat.Should().Be(1);
            afterFeat.Should().Be(1);
            mockExecutionStateManager.Verify(x => x.CheckforHalt(It.IsAny<IServiceProvider>(), It.IsAny<FeatureContext>(), TestThreadState.StartingFeature), Times.Once());

            scenarioStrategy.AddedScenarios.Should().BeEquivalentTo(scenarios);
        }

        private class MyScenarioStrategy : IScenarioExecutionStrategy
        {
            public List<(IScenarioInfo scenario, VariableSet variables)> AddedScenarios { get; } = new List<(IScenarioInfo, VariableSet)>();

            public ValueTask ExecuteAsync(IAutoStepServiceScope featureScope, FeatureContext featureContext, IScenarioInfo scenario, VariableSet variables, CancellationToken cancelToken)
            {
                AddedScenarios.Add((scenario, variables));

                return default;
            }
        }

        private class MyEventHandler : BaseEventHandler
        {
            private readonly Action<FeatureContext> callBefore;
            private readonly Action<FeatureContext> callAfter;
            private readonly Action<Exception>? exception;

            public MyEventHandler(Action<FeatureContext> callBefore, Action<FeatureContext> callAfter, Action<Exception>? exception = null)
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

            public override async ValueTask OnFeatureAsync(IServiceProvider scope, FeatureContext ctxt, Func<IServiceProvider, FeatureContext, CancellationToken, ValueTask> next, CancellationToken cancelToken)
            {
                callBefore(ctxt);

                try
                {
                    await next(scope, ctxt, cancelToken);
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
