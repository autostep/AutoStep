using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using AutoStep.Elements.Metadata;

namespace AutoStep.Execution.Results
{
    /// <summary>
    /// Contains the working result set, updated by the results collector.
    /// </summary>
    public class WorkingResultSet : IRunResultSet
    {
        private ConcurrentDictionary<string, FeatureResultData> featureDataLookup = new ConcurrentDictionary<string, FeatureResultData>();
        private SortedSet<IFeatureResult> orderedSet = new SortedSet<IFeatureResult>(Comparer<IFeatureResult>.Create(FeatureComparer));

        /// <inheritdoc/>
        public IEnumerable<IFeatureResult> Features => orderedSet;

        /// <inheritdoc/>
        public bool AllPassed => Features.All(x => x.Passed);

        /// <inheritdoc/>
        public DateTime StartTimeUtc { get; set; }

        /// <inheritdoc />
        public DateTime EndTimeUtc { get; set; }

        /// <summary>
        /// Adds a feature to the result set.
        /// </summary>
        /// <param name="feature">The feature info.</param>
        /// <param name="startTimeUtc">The start time (UTC) of the feature.</param>
        /// <returns>The feature data.</returns>
        public FeatureResultData AddFeature(IFeatureInfo feature, DateTime startTimeUtc)
        {
            if (feature is null)
            {
                throw new ArgumentNullException(nameof(feature));
            }

            var featureData = new FeatureResultData(feature, startTimeUtc);

            if (featureDataLookup.TryAdd(GetFeatureKey(feature), featureData))
            {
                lock (orderedSet)
                {
                    orderedSet.Add(featureData);
                }
            }
            else
            {
                throw new InvalidOperationException(WorkingResultSetMessages.FeatureAlreadyAdded);
            }

            return featureData;
        }

        /// <summary>
        /// Adds an individual scenario invocation to the result set.
        /// </summary>
        /// <param name="feature">The feature info.</param>
        /// <param name="scenario">The scenario info.</param>
        /// <param name="startTimeUtc">The start time of the invocation (UTC).</param>
        /// <param name="invocationName">The optional name for the individual invocation.</param>
        /// <param name="outlineVariables">The optional set of variables provided to the scenario invocation.</param>
        /// <returns>A set of scenario invocation data that can be updated on scenario completion.</returns>
        public ScenarioInvocationResultData AddScenarioInvocation(IFeatureInfo feature, IScenarioInfo scenario, DateTime startTimeUtc, string? invocationName, TableVariableSet? outlineVariables)
        {
            if (feature is null)
            {
                throw new ArgumentNullException(nameof(feature));
            }

            if (scenario is null)
            {
                throw new ArgumentNullException(nameof(scenario));
            }

            if (featureDataLookup.TryGetValue(GetFeatureKey(feature), out var featureData))
            {
                return featureData.AddScenarioInvocation(scenario, startTimeUtc, invocationName, outlineVariables);
            }
            else
            {
                throw new InvalidOperationException(WorkingResultSetMessages.FeatureNotAdded);
            }
        }

        /// <summary>
        /// Adds an individual scenario invocation to the result set.
        /// </summary>
        /// <param name="feature">The feature info.</param>
        /// <param name="scenario">The scenario info.</param>
        /// <param name="startTimeUtc">The start time of the invocation (UTC).</param>
        /// <returns>A set of scenario invocation data that can be updated on scenario completion.</returns>
        public ScenarioInvocationResultData AddScenarioInvocation(IFeatureInfo feature, IScenarioInfo scenario, DateTime startTimeUtc)
        {
            return AddScenarioInvocation(feature, scenario, startTimeUtc, null, null);
        }

        /// <summary>
        /// Get the unique key for looking up a specific feature.
        /// </summary>
        /// <param name="feature">The feature info.</param>
        /// <returns>A feature key.</returns>
        protected virtual string GetFeatureKey(IFeatureInfo feature)
        {
            if (feature is null)
            {
                throw new ArgumentNullException(nameof(feature));
            }

            return feature.SourceName ?? feature.Name;
        }

        private static int FeatureComparer(IFeatureResult left, IFeatureResult right)
        {
            string GetFeatureCompareValue(IFeatureInfo feature)
                => feature.SourceName ?? feature.Name;

            return string.Compare(GetFeatureCompareValue(left.Feature), GetFeatureCompareValue(right.Feature), StringComparison.CurrentCulture);
        }
    }
}
