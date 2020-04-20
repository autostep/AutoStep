using System.Threading;
using System.Threading.Tasks;
using AutoStep.Elements.Metadata;
using AutoStep.Execution.Contexts;
using AutoStep.Execution.Dependency;

namespace AutoStep.Execution.Strategy
{
    /// <summary>
    /// Defines the interface for a feature execution strategy, that defines the behaviour for executing each feature.
    /// </summary>
    public interface IFeatureExecutionStrategy
    {
        /// <summary>
        /// Execute the strategy.
        /// </summary>
        /// <param name="threadScope">The current service scope (which will be a thread scope).</param>
        /// <param name="threadContext">The test thread context.</param>
        /// <param name="feature">The feature metadata.</param>
        /// <param name="cancelToken">Cancellation token for the feature.</param>
        /// <returns>A task that should complete when the feature has finished executing.</returns>
        ValueTask ExecuteAsync(IAutoStepServiceScope threadScope, ThreadContext threadContext, IFeatureInfo feature, CancellationToken cancelToken);
    }
}
