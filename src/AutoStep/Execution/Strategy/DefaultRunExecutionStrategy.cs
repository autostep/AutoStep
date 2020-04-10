using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using AutoStep.Configuration;
using AutoStep.Elements.Metadata;
using AutoStep.Execution.Contexts;
using AutoStep.Execution.Control;
using AutoStep.Execution.Dependency;
using AutoStep.Execution.Events;
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
        /// <returns>A task that should complete when the test run has finished executing.</returns>
        public Task Execute(IAutoStepServiceScope runScope, RunContext runContext, FeatureExecutionSet executionSet)
        {
            runScope = runScope.ThrowIfNull(nameof(runScope));
            runContext = runContext.ThrowIfNull(nameof(runContext));
            executionSet = executionSet.ThrowIfNull(nameof(executionSet));

            // Event handlers have all executed now.

            // Create a queue of all features.
            var featureQueue = new ConcurrentQueue<IFeatureInfo>(executionSet.Features);

            var parallelConfig = runContext.Configuration.GetRunConfigurationOption("parallelCount", 1);

            if (parallelConfig < 1)
            {
                throw new ProjectConfigurationException(StrategyMessages.ParallelCountMustBeGreaterThanZero);
            }

            var parallelValue = Math.Min(featureQueue.Count, parallelConfig);

            // Create x tasks based on level of parallelism.
            var parallelTasks = new Task[parallelValue];

            IFeatureInfo? FeatureDeQueue(ConcurrentQueue<IFeatureInfo> queue)
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
                parallelTasks[idx] = Task.Run(() => TestThreadFeatureParallel(runScope, threadId, () => FeatureDeQueue(featureQueue)));
            }

            // Wait for test threads to finish.
            return Task.WhenAll(parallelTasks);
        }

        private async Task TestThreadFeatureParallel(IAutoStepServiceScope runScope, int testThreadId, Func<IFeatureInfo?> nextFeature)
        {
            var threadContext = new ThreadContext(testThreadId);

            using var threadScope = runScope.BeginNewScope(ScopeTags.ThreadTag, threadContext);

            var executionManager = threadScope.GetRequiredService<IExecutionStateManager>();
            var featureStrategy = threadScope.GetRequiredService<IFeatureExecutionStrategy>();
            var logger = threadScope.GetRequiredService<ILogger<DefaultRunExecutionStrategy>>();
            var events = threadScope.GetRequiredService<IEventPipeline>();

            await events.InvokeEvent(
                threadScope,
                threadContext,
                (handler, sc, ctxt, next) => handler.OnThread(sc, ctxt, next),
                async (_, ctxt) =>
                {
                    var haltInstruction = await executionManager.CheckforHalt(threadScope, ctxt, TestThreadState.Starting).ConfigureAwait(false);

                    // TODO: Do something with halt instruction (terminate, for example?).
                    while (true)
                    {
                        var feature = nextFeature();

                        if (feature is object)
                        {
                            logger.LogDebug("Test Thread ID {0}; executing feature '{1}'", testThreadId, feature.Name);

                            // We have a feature.
                            await featureStrategy.Execute(threadScope, threadContext, feature).ConfigureAwait(false);
                        }
                        else
                        {
                            // TODO: Logging.
                            logger.LogDebug("Test Thread ID {0}; no more features to run.", testThreadId);
                            break;
                        }
                    }
                }).ConfigureAwait(false);
        }
    }
}
