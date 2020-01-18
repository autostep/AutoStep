using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using AutoStep.Definitions;
using AutoStep.Elements;
using AutoStep.Tracing;

namespace AutoStep.Execution
{
    internal interface IStepCollectionExecutionStrategy
    {
        Task Execute(ErrorCapturingContext owningContext, StepCollectionElement stepCollection, VariableSet variables, EventManager events, IExecutionStateManager executionManager);
    }

    internal class StepCollectionExecutionStrategy : IStepCollectionExecutionStrategy
    {
        public async Task Execute(ErrorCapturingContext owningContext, StepCollectionElement stepCollection, VariableSet variables, EventManager events, IExecutionStateManager executionManager)
        {
            for (var stepIdx = 0; stepIdx < stepCollection.Steps.Count; stepIdx++)
            {
                var step = stepCollection.Steps[stepIdx];

                using var stepContext = new StepContext(owningContext, step, variables);

                // Halt before the step begins.
                var stepHaltInstruction = await executionManager.CheckforHalt(owningContext, TestThreadState.StartingStep).ConfigureAwait(false);

                // Halt instruction for step collections can include:
                //  - Moving to a specific step position
                //  - Stepping Up (i.e. run to next scope).
                //  - Something else?

                try
                {
                    await using (await events.EnterEventScope(stepContext, (h, ctxt) => h.BeginStep(ctxt), (h, ctxt) => h.EndStep(ctxt)))
                    {
                        var timer = new Stopwatch();

                        timer.Start();

                        try
                        {
                            // Execute step.
                            // TODO!
                        }
                        catch (Exception ex)
                        {
                            // Record the exception in the step context, so it's available to event handlers.
                            stepContext.FailException = ex;
                        }
                        finally
                        {
                            timer.Stop();
                            stepContext.Elapsed = timer.Elapsed;
                        }
                    }
                }
                catch (EventHandlingException ex)
                {
                    if (stepContext.FailException is null)
                    {
                        stepContext.FailException = new EventHandlingException(ex);
                    }
                    else
                    {
                        stepContext.FailException = new AggregateException(stepContext.FailException, ex);
                    }
                }

                if (stepContext.FailException is object)
                {
                    // The step failed, alert the 
                }
            }
        }
    }

    internal class DefaultScenarioExecutionStrategy : IScenarioExecutionStrategy
    {
        private readonly ITracer tracer;
        private readonly IStepCollectionExecutionStrategy collectionExecutionStrategy;

        public DefaultScenarioExecutionStrategy(ITracer tracer, IStepCollectionExecutionStrategy? stepCollectionExecutionStrategy = null)
        {
            this.tracer = tracer;
            collectionExecutionStrategy = stepCollectionExecutionStrategy ?? new StepCollectionExecutionStrategy();
        }

        public async Task Execute(FeatureContext featureContext, ScenarioElement scenario, VariableSet variableSet, EventManager events, IExecutionStateManager executionManager)
        {
            using var scenarioContext = new ScenarioContext(featureContext, scenario, variableSet);

            // Halt before the scenario begins.
            var haltInstruction = await executionManager.CheckforHalt(scenarioContext, TestThreadState.StartingScenario).ConfigureAwait(false);

            var invokedFailureEvent = false;

            try
            {
                await using (await events.EnterEventScope(scenarioContext, (h, ctxt) => h.BeginScenario(ctxt), (h, ctxt) => h.EndScenario(ctxt)))
                {
                    // Any errors will be udated on the scenario context.
                    await collectionExecutionStrategy.Execute(scenarioContext, scenario, variableSet, events, executionManager);

                    if (scenarioContext.FailException is null)
                    {
                        events.InvokeEvent(scenarioContext);
                    }
                }
            }
            catch (Exception ex)
            {
                // Scenario setup errors should just be logged.
                tra
            }
        }
    }
}
