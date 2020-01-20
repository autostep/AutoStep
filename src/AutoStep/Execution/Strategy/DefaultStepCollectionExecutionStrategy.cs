using System;
using System.Diagnostics;
using System.Threading.Tasks;
using AutoStep.Elements;
using AutoStep.Elements.ReadOnly;
using AutoStep.Execution.Control;
using AutoStep.Execution.Dependency;

namespace AutoStep.Execution.Strategy
{
    internal class DefaultStepCollectionExecutionStrategy : IStepCollectionExecutionStrategy
    {
        public async Task Execute(IServiceScope owningScope, ErrorCapturingContext owningContext, IStepCollectionInfo stepCollection, VariableSet variables)
        {
            // Resolve the thread context, so we can access the stack of steps.
            var threadContext = owningScope.ThreadContext();

            var stepExecutionStrategy = owningScope.Resolve<IStepExecutionStrategy>();
            var executionManager = owningScope.Resolve<IExecutionStateManager>();
            var events = owningScope.Resolve<IEventPipeline>();

            for (var stepIdx = 0; stepIdx < stepCollection.Steps.Count; stepIdx++)
            {
                var step = stepCollection.Steps[stepIdx];

                var stepContext = new StepContext(stepIdx, owningContext, step, variables);

                using var stepScope = owningScope.BeginNewScope(ScopeTags.StepTag, stepContext);

                // Halt before the step begins.
                var stepHaltInstruction = await executionManager.CheckforHalt(stepScope, owningContext, TestThreadState.StartingStep).ConfigureAwait(false);

                // Halt instruction for step collections can include:
                //  - Moving to a specific step position
                //  - Stepping Up (i.e. run to next scope).
                //  - Something else?
                await events.InvokeEvent(
                    stepScope,
                    stepContext,
                    (handler, sc, ctxt, next) => handler.Step(sc, ctxt, next),
                    async (scope, ctxt) =>
                    {
                        var timer = new Stopwatch();

                        timer.Start();

                        try
                        {
                            // Execute the step.
                            await stepExecutionStrategy.ExecuteStep(
                                scope,
                                ctxt,
                                variables).ConfigureAwait(false);
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
