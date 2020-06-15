using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using AutoStep.Configuration;
using AutoStep.Elements.Metadata;
using AutoStep.Execution.Contexts;
using AutoStep.Execution.Control;
using AutoStep.Execution.Dependency;
using AutoStep.Execution.Events;
using AutoStep.Execution.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AutoStep.Execution.Strategy
{
    /// <summary>
    /// Implements the default run execution strategy.
    /// </summary>
    public class DefaultRunExecutionStrategy : IRunExecutionStrategy
    {
        /// <summary>
        /// Defines the ID of the key used for the configured parallel count.
        /// </summary>
        public const string ParallelExecutionConfigurationKey = "parallelCount";

        /// <summary>
        /// Execute the strategy.
        /// </summary>
        /// <param name="runScope">The top-level run scope.</param>
        /// <param name="runContext">The run context.</param>
        /// <param name="executionSet">The set of all features and scenarios to test.</param>
        /// <param name="cancelToken">Cancellation token for the run.</param>
        /// <returns>A task that should complete when the test run has finished executing.</returns>
        public Task ExecuteAsync(ILifetimeScope runScope, RunContext runContext, FeatureExecutionSet executionSet, CancellationToken cancelToken)
        {
            runScope = runScope.ThrowIfNull(nameof(runScope));
            runContext = runContext.ThrowIfNull(nameof(runContext));
            executionSet = executionSet.ThrowIfNull(nameof(executionSet));

            // Event handlers have all executed now.

            // Create a queue of all features.
            var featureQueue = new ConcurrentQueue<IFeatureInfo>(executionSet.Features);

            var parallelConfig = runContext.Configuration.GetRunValue("parallelCount", 1);

            if (parallelConfig < 1)
            {
                throw new ProjectConfigurationException(StrategyMessages.ParallelCountMustBeGreaterThanZero);
            }

            var parallelValue = Math.Min(featureQueue.Count, parallelConfig);

            // Create x tasks based on level of parallelism.
            var parallelTasks = new Task[parallelValue];

            static IFeatureInfo? FeatureDeQueue(ConcurrentQueue<IFeatureInfo> queue)
            {
                if (queue.TryDequeue(out var result))
                {
                    return result;
                }

                return null;
            }

            for (int idx = 0; idx < parallelValue; idx++)
            {
                var threadId = idx + 1;

                // Initially we'll just go for a feature parallel, but eventually we will
                // probably add support for a scenario parallel.
                parallelTasks[idx] = Task.Run(() => TestThreadFeatureParallel(runScope, threadId, () => FeatureDeQueue(featureQueue), cancelToken));
            }

            // Wait for test threads to finish.
            return Task.WhenAll(parallelTasks);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Design",
            "CA1031:Do not catch general exception types",
            Justification = "Need a 'last possible catch' handler for unexpected errors.")]
        private async Task TestThreadFeatureParallel(ILifetimeScope runScope, int testThreadId, Func<IFeatureInfo?> nextFeature, CancellationToken cancelToken)
        {
            var threadContext = new ThreadContext(testThreadId);

            using var threadScope = runScope.BeginContextScope(ScopeTags.ThreadTag, threadContext);

            var executionManager = threadScope.Resolve<IExecutionStateManager>();
            var featureStrategy = threadScope.Resolve<IFeatureExecutionStrategy>();
            var logger = threadScope.Resolve<ILogger<DefaultRunExecutionStrategy>>();
            var events = threadScope.Resolve<IEventPipeline>();
            var contextProvider = threadScope.Resolve<IContextScopeProvider>();

            using (contextProvider.EnterContextScope(threadContext))
            {
                await events.InvokeEventAsync(
                    threadScope,
                    threadContext,
                    (handler, sc, ctxt, next, cancel) => handler.OnThreadAsync(sc, ctxt, next, cancel),
                    cancelToken,
                    async (_, ctxt, cancel) =>
                    {
                        var haltInstruction = await executionManager.CheckforHalt(threadScope, ctxt, TestThreadState.Starting).ConfigureAwait(false);

                        // TODO: Do something with halt instruction (terminate, for example?).
                        while (true)
                        {
                            var feature = nextFeature();

                            if (feature is object)
                            {
                                logger.LogDebug("Test Thread ID {0}; executing feature '{1}'", testThreadId, feature.Name);

                                try
                                {
                                    // We have a feature.
                                    await featureStrategy.ExecuteAsync(threadScope, threadContext, feature, cancel).ConfigureAwait(false);
                                }
                                catch (Exception ex)
                                {
                                    logger.LogError(ex, "Unhandled Exception during execution of feature '{0}': {1}", feature.Name, ex.Message);
                                }
                            }
                            else
                            {
                                logger.LogDebug("Test Thread ID {0}; no more features to run.", testThreadId);
                                break;
                            }
                        }
                    }).ConfigureAwait(false);
            }
        }
    }
}
