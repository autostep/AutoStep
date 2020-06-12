using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using AutoStep.Elements.Metadata;

namespace AutoStep.Execution.Results
{
    /// <summary>
    /// Contains the working set of results for a feature.
    /// </summary>
    public class FeatureResultData : IFeatureResult
    {
        private readonly ConcurrentDictionary<string, ScenarioResultData> scenarioLookup;
        private readonly SortedSet<IScenarioResult> orderedSet;

        /// <summary>
        /// Initializes a new instance of the <see cref="FeatureResultData"/> class.
        /// </summary>
        /// <param name="featureInfo">The <see cref="IFeatureInfo"/> for the feature.</param>
        /// <param name="startTimeUtc">The start time (in UTC) of the feature.</param>
        public FeatureResultData(IFeatureInfo featureInfo, DateTime startTimeUtc)
        {
            scenarioLookup = new ConcurrentDictionary<string, ScenarioResultData>();
            orderedSet = new SortedSet<IScenarioResult>(Comparer<IScenarioResult>.Create(CompareScenarios));
            Feature = featureInfo;
            StartTimeUtc = startTimeUtc;
        }

        /// <inheritdoc/>
        public IEnumerable<IScenarioResult> Scenarios => orderedSet;

        /// <inheritdoc/>
        public IFeatureInfo Feature { get; }

        /// <inheritdoc/>
        public bool Passed => FeatureFailureException is null && Scenarios.All(x => x.Passed);

        /// <inheritdoc/>
        public DateTime StartTimeUtc { get; }

        /// <inheritdoc/>
        public DateTime EndTimeUtc { get; set; }

        /// <inheritdoc/>
        public Exception? FeatureFailureException { get; set; }

        /// <summary>
        /// Add a single scenario invocation to the feature's result set.
        /// </summary>
        /// <param name="scenario">The <see cref="IScenarioInfo"/> for the scenario.</param>
        /// <param name="startTimeUtc">The start time of the scenario invocation (in UTC).</param>
        /// <param name="invocationName">An optional name for the individual invocation of the scenario.</param>
        /// <param name="outlineVariables">The set of variables passed into the scenario outline.</param>
        /// <returns>A new block of scenario invocation data, to update when the scenario finishes.</returns>
        public ScenarioInvocationResultData AddScenarioInvocation(IScenarioInfo scenario, DateTime startTimeUtc, string? invocationName, TableVariableSet? outlineVariables)
        {
            if (scenario is null)
            {
                throw new ArgumentNullException(nameof(scenario));
            }

            var scenarioData = scenarioLookup.GetOrAdd(scenario.Name, k =>
            {
                var newData = new ScenarioResultData(scenario);

                // Scenarios could be added from multiple threads.
                lock (orderedSet)
                {
                    orderedSet.Add(newData);
                }

                return newData;
            });

            return scenarioData.AddInvocation(startTimeUtc, invocationName, outlineVariables);
        }

        private static int CompareScenarios(IScenarioResult left, IScenarioResult right)
        {
            // Scenarios should be sorted by their position in the file.
            return left.Scenario.SourceLine.CompareTo(right.Scenario.SourceLine);
        }
    }
}
