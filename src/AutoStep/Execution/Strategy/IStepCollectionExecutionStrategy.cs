using System.Threading;
using System.Threading.Tasks;
using Autofac;
using AutoStep.Elements.Metadata;
using AutoStep.Execution.Contexts;
using AutoStep.Execution.Dependency;

namespace AutoStep.Execution.Strategy
{
    /// <summary>
    /// Defines the interface for a step collection execution strategy, that controls the behaviour of executing a set of steps,
    /// typically from a scenario, but also from defined steps that invoke other steps.
    /// </summary>
    public interface IStepCollectionExecutionStrategy
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
        ValueTask ExecuteAsync(
            ILifetimeScope owningScope,
            StepCollectionContext owningContext,
            IStepCollectionInfo stepCollection,
            VariableSet variables,
            CancellationToken cancelToken);
    }
}
