using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using AutoStep.Elements.Metadata;
using AutoStep.Execution.Contexts;
using AutoStep.Execution.Control;
using AutoStep.Execution.Dependency;
using AutoStep.Execution.Events;
using AutoStep.Execution.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace AutoStep.Execution.Strategy
{
    /// <summary>
    /// Implements the default step collection execution strategy.
    /// </summary>
    internal class DefaultStepCollectionExecutionStrategy : IStepCollectionExecutionStrategy
    {
        /// <summary>
        /// Execute the strategy.
        /// </summary>
        /// <param name="owningScope">The owning scope.</param>
        /// <param name="owningContext">The owning context.</param>
        /// <param name="stepCollection">The step collection metadata.</param>
        /// <param name="variables">The set of variables currently in-scope.</param>
        /// <param name="cancelToken">Cancellation token for the step collection.</param>
        /// <returns>A task that should complete when the step collection has finished executing.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Need to capture any error arising from a nested step.")]
        public async ValueTask ExecuteAsync(IAutoStepServiceScope owningScope, StepCollectionContext owningContext, IStepCollectionInfo stepCollection, VariableSet variables, CancellationToken cancelToken)
        {
            var stepExecutionStrategy = owningScope.GetRequiredService<IStepExecutionStrategy>();
            var executionManager = owningScope.GetRequiredService<IExecutionStateManager>();
            var events = owningScope.GetRequiredService<IEventPipeline>();
            var contextScopeProvider = owningScope.GetRequiredService<IContextScopeProvider>();

            for (var stepIdx = 0; stepIdx < stepCollection.Steps.Count; stepIdx++)
            {
                var step = stepCollection.Steps[stepIdx];

                var stepContext = new StepContext(stepIdx, owningContext, step, variables);

                using (contextScopeProvider.EnterContextScope(stepContext))
                {
                    IAutoStepServiceScope? locallyOwnedScope = null;
                    IAutoStepServiceScope stepScope;

                    if (owningScope.Tag == ScopeTags.StepTag)
                    {
                        stepScope = owningScope;
                    }
                    else
                    {
                        // The parent scope is not a step. Create a new one.
                        stepScope = locallyOwnedScope = owningScope.BeginNewScope(ScopeTags.StepTag, stepContext);
                    }

                    var stepRan = false;

                    var timer = new Stopwatch();
                    timer.Start();

                    try
                    {
                        cancelToken.ThrowIfCancellationRequested();

                        // TODO: Halt before the step begins.
                        // Halt instruction for step collections can include:
                        //  - Moving to a specific step position
                        //  - Stepping Up (i.e. run to next scope).
                        //  - Something else?
                        var stepHaltInstruction = await executionManager.CheckforHalt(stepScope, stepContext, TestThreadState.StartingStep).ConfigureAwait(false);

                        await events.InvokeEventAsync(
                            stepScope,
                            stepContext,
                            (handler, sc, ctxt, next, cancel) => handler.OnStepAsync(sc, ctxt, next, cancel),
                            cancelToken,
                            async (_, ctxt, cancel) =>
                            {
                                try
                                {
                                    stepRan = true;

                                    // Execute the step.
                                    await stepExecutionStrategy.ExecuteStepAsync(
                                                stepScope,
                                                ctxt,
                                                variables,
                                                cancel).ConfigureAwait(false);
                                }
                                catch (EventHandlingException ex)
                                {
                                    stepContext.FailException = ex;
                                }
                                catch (StepFailureException ex)
                                {
                                    stepContext.FailException = ex;
                                }
                                catch (OperationCanceledException ex)
                                {
                                    stepContext.FailException = ex;
                                }
                                catch (Exception ex)
                                {
                                // Wrap the context.
                                stepContext.FailException = new StepFailureException(stepContext.Step, ex);
                                }
                            }).ConfigureAwait(false);
                    }
                    catch (EventHandlingException ex)
                    {
                        // Error in an event handler; fail the step.
                        stepContext.FailException = ex;
                    }
                    catch (OperationCanceledException ex)
                    {
                        stepContext.FailException = ex;
                    }
                    finally
                    {
                        timer.Stop();
                        stepContext.Elapsed = timer.Elapsed;
                        stepContext.StepExecuted = stepRan;

                        // Dispose of the locally owned scope (if we created one).
                        locallyOwnedScope?.Dispose();
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
        }
    }
}
