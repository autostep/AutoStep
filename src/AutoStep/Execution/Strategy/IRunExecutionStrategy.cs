using System.Threading;
using System.Threading.Tasks;
using AutoStep.Execution.Contexts;
using AutoStep.Execution.Dependency;

namespace AutoStep.Execution.Strategy
{
    /// <summary>
    /// Defines the interface for a run execution strategy, that defines the behaviour for executing an entire test run.
    /// </summary>
    /// <remarks>You can implement a custom run strategy to change the threading behaviour.</remarks>
    public interface IRunExecutionStrategy
    {
        /// <summary>
        /// Execute the strategy.
        /// </summary>
        /// <param name="runScope">The top-level run scope.</param>
        /// <param name="runContext">The run context.</param>
        /// <param name="executionSet">The set of all features and scenarios to test.</param>
        /// <param name="cancelToken">Cancellation token for the run.</param>
        /// <returns>A task that should complete when the test run has finished executing.</returns>
        Task ExecuteAsync(IAutoStepServiceScope runScope, RunContext runContext, FeatureExecutionSet executionSet, CancellationToken cancelToken);
    }
}
