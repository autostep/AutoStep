using System;
using System.Diagnostics;
using System.Threading.Tasks;
using AutoStep.Elements;
using AutoStep.Elements.ReadOnly;
using AutoStep.Execution.Contexts;
using AutoStep.Execution.Control;
using AutoStep.Execution.Dependency;
using AutoStep.Execution.Events;

namespace AutoStep.Execution.Strategy
{
    internal class DefaultStepCollectionExecutionStrategy : IStepCollectionExecutionStrategy
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Need to capture any error arising from a nested step.")]
        public async Task Execute(IServiceScope owningScope, StepCollectionContext owningContext, IStepCollectionInfo stepCollection, VariableSet variables)
        {
            // Resolve the thread context, so we can access the stack of steps.
            var threadContext = owningScope.ThreadContext();

            var stepExecutionStrategy = owningScope.Resolve<IStepExecutionStrategy>();
            var executionManager = owningScope.Resolve<IExecutionStateManager>();
            var events = owningScope.Resolve<IEventPipeline>();

            var collectionTimer = new Stopwatch();
            collectionTimer.Start();

            try
            {
                for (var stepIdx = 0; stepIdx < stepCollection.Steps.Count; stepIdx++)
                {
                    var step = stepCollection.Steps[stepIdx];

                    var stepContext = new StepContext(stepIdx, owningContext, step, variables);

                    using var stepScope = owningScope.BeginNewScope(stepContext);

                    // Halt before the step begins.
                    var stepHaltInstruction = await executionManager.CheckforHalt(stepScope, stepContext, TestThreadState.StartingStep).ConfigureAwait(false);

                    try
                    {
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
                                catch (EventHandlingException ex)
                                {
                                    stepContext.FailException = ex;
                                }
                                catch (StepFailureException ex)
                                {
                                    stepContext.FailException = ex;
                                }
                                catch (Exception ex)
                                {
                                    // Wrap the context.
                                    stepContext.FailException = new StepFailureException(stepContext.Step, ex);
                                }
                                finally
                                {
                                    timer.Stop();
                                    stepContext.Elapsed = timer.Elapsed;
                                }

                            }).ConfigureAwait(false);
                    }
                    catch (EventHandlingException ex)
                    {
                        // Error in an event handler; fail the step.
                        stepContext.FailException = ex;
                    }

                    if (stepContext.FailException is object)
                    {
                        // The step failed, alert the execution manager.
                        var breakInstructions = await executionManager.StepError(stepContext).ConfigureAwait(false);

                        // React to the 'break'. Retry step?
                        // Can we re-bind/recompile partway through a test?
                        // Re-linking won't be an issue, because all that will change is the step definition,
                        // but re-compilation will re-construct our tree structure. Perhaps there is a way for the
                        // break response to instruct the caller.
                        // Regardless, we need to mark the owner as failing.
                        owningContext.FailException = stepContext.FailException;
                        owningContext.FailingStep = stepContext.Step;

                        // Consider allowing the scenario to continue until the next non-assert step (i.e. Then/When, etc).
                        break;
                    }
                }
            }
            finally
            {
                owningContext.Elapsed = collectionTimer.Elapsed;
                collectionTimer.Stop();
            }
        }
    }
}
