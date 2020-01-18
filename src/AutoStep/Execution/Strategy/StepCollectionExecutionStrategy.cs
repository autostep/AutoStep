using System;
using System.Diagnostics;
using System.Threading.Tasks;
using AutoStep.Elements;
using AutoStep.Execution.Control;

namespace AutoStep.Execution.Strategy
{
    internal class StepCollectionExecutionStrategy : IStepCollectionExecutionStrategy
    {
        private readonly IStepExecutionStrategy stepExecutionStrategy;

        public StepCollectionExecutionStrategy(IStepExecutionStrategy? stepExecutionStrategy = null)
        {
            this.stepExecutionStrategy = stepExecutionStrategy ?? new DefaultStepExecutionStrategy();
        }

        public async Task Execute(ErrorCapturingContext owningContext, StepCollectionElement stepCollection, VariableSet variables, EventManager events, IExecutionStateManager executionManager)
        {
            // Resolve the thread context, so we can access the stack of steps.
            var threadContext = owningContext.Scope.Resolve<ThreadContext>();

            for (var stepIdx = 0; stepIdx < stepCollection.Steps.Count; stepIdx++)
            {
                var step = stepCollection.Steps[stepIdx];

                using var stepContext = new StepContext(owningContext, stepIdx, step, variables);

                // Halt before the step begins.
                var stepHaltInstruction = await executionManager.CheckforHalt(owningContext, TestThreadState.StartingStep).ConfigureAwait(false);

                // Halt instruction for step collections can include:
                //  - Moving to a specific step position
                //  - Stepping Up (i.e. run to next scope).
                //  - Something else?

                await events.InvokeEvent(stepContext, (handler, ctxt, next) => handler.Step(ctxt, next), async ctxt =>
                {
                    var timer = new Stopwatch();

                    timer.Start();

                    try
                    {
                        // Execute the step.
                        await stepExecutionStrategy.ExecuteStep(stepContext, variables, events, executionManager, this).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        // Record the exception in the step context, so it's available to event handlers.
                        ctxt.FailException = ex;
                    }
                    finally
                    {
                        timer.Stop();
                        ctxt.Elapsed = timer.Elapsed;
                    }

                }).ConfigureAwait(false);

                if (stepContext.FailException is object)
                {
                    // The step failed, alert the execution manager.
                    var breakInstructions = await executionManager.StepError(stepContext).ConfigureAwait(false);

                    // React to the 'break'. Retry step?
                    // Can we re-bind/recompile partway through a test?
                    // If that happens, the underlying step collection will change, and potentially, the definition,
                    // but other than that, we could actually suspend the test here, modify, recompile and link, then
                    // just continue the test.
                    // Failure to compile could just result in an abort choice?
                }
            }
        }
    }
}
