using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoStep.Elements.ReadOnly;
using AutoStep.Execution;
using AutoStep.Execution.Contexts;
using AutoStep.Execution.Control;
using AutoStep.Execution.Dependency;
using AutoStep.Execution.Events;
using AutoStep.Execution.Strategy;
using AutoStep.Tests.Builders;
using AutoStep.Tests.Utils;
using FluentAssertions;
using Microsoft.VisualStudio.TestPlatform.PlatformAbstractions;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace AutoStep.Tests.Execution.Strategy
{
    public class DefaultStepCollectionExecutionStrategyTests : LoggingTestBase
    {
        public DefaultStepCollectionExecutionStrategyTests(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }

        [Fact]
        public async ValueTask RunsAllSteps()
        {
            var stepCollection = new ScenarioBuilder("My Scenario", 1, 1)
                                     .Given("I have done something", 2, 1)
                                     .Then("Something else happens", 3, 1)
                                     .And("a really big thing happens", StepType.Then, 4, 1)
                                     .Built;

            var variables = new VariableSet();

            var owningContext = new ScenarioContext(stepCollection, variables);

            int ranSteps = 0;

            await DoTest(stepCollection, 3, owningContext, variables, (scope, ctxt, varSet) =>
            {
                scope.Tag.Should().Be(ScopeTags.GeneralScopeTag);
                ctxt.StepIndex.Should().Be(ranSteps);
                ctxt.Step.Should().Be(stepCollection.Steps[ranSteps]);
                ctxt.Variables.Should().Be(variables);
                varSet.Should().Be(variables);
                ctxt.FailException.Should().BeNull();

                ranSteps++;
            });

            ranSteps.Should().Be(3);
        }

        [Fact]
        public async ValueTask StopsTestIfStepFails()
        {
            var stepCollection = new ScenarioBuilder("My Scenario", 1, 1)
                                     .Given("I have done something", 2, 1)
                                     .Then("Something else happens", 3, 1)
                                     .And("a really big thing happens", StepType.Then, 4, 1)
                                     .Built;

            var variables = new VariableSet();

            var owningContext = new ScenarioContext(stepCollection, variables);

            int ranSteps = 0;

            await DoTest(stepCollection, 2, owningContext, variables, (scope, ctxt, varSet) =>
            {
                scope.Tag.Should().Be(ScopeTags.GeneralScopeTag);
                ctxt.StepIndex.Should().Be(ranSteps);
                ctxt.Step.Should().Be(stepCollection.Steps[ranSteps]);
                ctxt.Variables.Should().Be(variables);
                varSet.Should().Be(variables);
                
                ranSteps++;

                if (ctxt.Step.Type == StepType.Then)
                {
                    // Throw.
                    throw new NullReferenceException();
                }
            });

            ranSteps.Should().Be(2);

            owningContext.FailException.Should().BeOfType<StepFailureException>();
            owningContext.FailException.InnerException.Should().BeOfType<NullReferenceException>();
            owningContext.FailingStep.Should().Be(stepCollection.Steps[1]);
        }

        [Fact]
        public async ValueTask StopsTestIfEventHandlerFails()
        {
            var stepCollection = new ScenarioBuilder("My Scenario", 1, 1)
                                     .Given("I have done something", 2, 1)
                                     .Then("Something else happens", 3, 1)
                                     .And("a really big thing happens", StepType.Then, 4, 1)
                                     .Built;

            var variables = new VariableSet();

            var owningContext = new ScenarioContext(stepCollection, variables);

            int ranSteps = 0;

            var beforeFeat = 0;
            var afterFeat = 0;
            var eventHandler = new MyEventHandler((StepContext ctxt) =>
            {
                beforeFeat++;

                if (beforeFeat == 2)
                {
                    throw new NullReferenceException();
                }
            }, c => afterFeat++);

            await DoTest(stepCollection, 2, owningContext, variables, eventHandler, (scope, ctxt, varSet) =>
            {
                scope.Tag.Should().Be(ScopeTags.GeneralScopeTag);
                ctxt.StepIndex.Should().Be(ranSteps);
                ctxt.Step.Should().Be(stepCollection.Steps[ranSteps]);
                ctxt.Variables.Should().Be(variables);
                varSet.Should().Be(variables);

                ranSteps++;
            });

            ranSteps.Should().Be(1);

            beforeFeat.Should().Be(2);
            afterFeat.Should().Be(1);

            owningContext.FailException.Should().BeOfType<EventHandlingException>();
            owningContext.FailException.InnerException.Should().BeOfType<NullReferenceException>();
            owningContext.FailingStep.Should().Be(stepCollection.Steps[1]);
        }
               
        private async ValueTask DoTest(
            IStepCollectionInfo stepCollection,
            int executionManagerInvokes,
            StepCollectionContext owningContext,
            VariableSet variables,
            Action<IServiceScope, StepContext, VariableSet> stepCallback)
        {
            var beforeFeat = 0;
            var afterFeat = 0;
            var eventHandler = new MyEventHandler((StepContext ctxt) =>
            {
                beforeFeat++;
            }, c => afterFeat++);

            await DoTest(stepCollection, executionManagerInvokes, owningContext, variables, eventHandler, stepCallback);
        }

        private async ValueTask DoTest(
            IStepCollectionInfo stepCollection,
            int executionManagerInvokes,
            StepCollectionContext owningContext,
            VariableSet variables,
            IEventHandler eventHandler,
            Action<IServiceScope, StepContext, VariableSet> stepCallback)
        {
            var threadContext = new ThreadContext(1);
            var mockExecutionStateManager = new Mock<IExecutionStateManager>();
            
            var eventPipeline = new EventPipeline(new List<IEventHandler> { eventHandler });
            var stepExecuteStrategy = new MyStepExecutionStrategy(stepCallback);

            var builder = new AutofacServiceBuilder();

            builder.RegisterSingleInstance(threadContext);
            builder.RegisterSingleInstance(LogFactory);
            builder.RegisterSingleInstance(mockExecutionStateManager.Object);
            builder.RegisterSingleInstance<IEventPipeline>(eventPipeline);
            builder.RegisterSingleInstance<IStepExecutionStrategy>(stepExecuteStrategy);

            var scope = builder.BuildRootScope();

            var strategy = new DefaultStepCollectionExecutionStrategy();

            await strategy.Execute(scope, owningContext, stepCollection, variables);

            owningContext.Elapsed.TotalMilliseconds.Should().BeGreaterThan(0);

            mockExecutionStateManager.Verify(x => x.CheckforHalt(It.IsAny<IServiceScope>(), It.IsAny<StepContext>(), TestThreadState.StartingStep), 
                                             Times.Exactly(executionManagerInvokes));

            if(owningContext.FailException != null)
            {
                mockExecutionStateManager.Verify(x => x.StepError(It.IsAny<StepContext>()), Times.Once());
            }
        }

        private class MyStepExecutionStrategy : IStepExecutionStrategy
        {
            private readonly Action<IServiceScope, StepContext, VariableSet> stepCallback;

            public MyStepExecutionStrategy(Action<IServiceScope, StepContext, VariableSet> stepCallback)
            {
                this.stepCallback = stepCallback;
            }
            
            public ValueTask ExecuteStep(IServiceScope stepScope, StepContext context, VariableSet variables)
            {
                stepScope.Should().NotBeNull();
                stepScope.Tag.Should().Be(ScopeTags.GeneralScopeTag);

                stepCallback(stepScope, context, variables);

                return default;
            }
        }

        private class MyEventHandler : IEventHandler
        {
            private readonly Action<StepContext> callBefore;
            private readonly Action<StepContext> callAfter;
            private readonly Action<Exception> exception;

            public MyEventHandler(Action<StepContext> callBefore, Action<StepContext> callAfter, Action<Exception> exception = null)
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

            public ValueTask Feature(IServiceScope scope, FeatureContext ctxt, Func<IServiceScope, FeatureContext, ValueTask> next)
            {
                throw new NotImplementedException();
            }

            public ValueTask Thread(IServiceScope scope, ThreadContext ctxt, Func<IServiceScope, ThreadContext, ValueTask> next)
            {
                throw new NotImplementedException();
            }

            public ValueTask Execute(IServiceScope scope, RunContext ctxt, Func<IServiceScope, RunContext, ValueTask> next)
            {
                throw new NotImplementedException();
            }

            public void ConfigureServices(IServicesBuilder builder, RunConfiguration configuration)
            {
                throw new NotImplementedException();
            }
            
            public  ValueTask Scenario(IServiceScope scope, ScenarioContext ctxt, Func<IServiceScope, ScenarioContext, ValueTask> next)
            {
                throw new NotImplementedException();
            }

            public async ValueTask Step(IServiceScope scope, StepContext ctxt, Func<IServiceScope, StepContext, ValueTask> next)
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

                ctxt.Elapsed.TotalMilliseconds.Should().BeGreaterThan(0);

                callAfter(ctxt);
            }

        }
    }
}
