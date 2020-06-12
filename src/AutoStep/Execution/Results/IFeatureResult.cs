using System;
using System.Collections.Generic;
using AutoStep.Elements.Metadata;

namespace AutoStep.Execution.Results
{
    /// <summary>
    /// Defines the result data for a single feature.
    /// </summary>
    public interface IFeatureResult
    {
        /// <summary>
        /// Gets a value indicating whether the feature executed successfully, and all the scenario invocations within the entire feature passed.
        /// </summary>
        public bool Passed { get; }

        /// <summary>
        /// Gets the compiled feature information.
        /// </summary>
        public IFeatureInfo Feature { get; }

        /// <summary>
        /// Gets the time (in UTC) at which the feature started executing.
        /// </summary>
        public DateTime StartTimeUtc { get; }

        /// <summary>
        /// Gets the time (in UTC) at which the feature finished executing.
        /// </summary>
        public DateTime EndTimeUtc { get; }

        /// <summary>
        /// Gets an exception that caused the feature to fail before any scenarios were run.
        /// </summary>
        Exception? FeatureFailureException { get; }

        /// <summary>
        /// Gets all the scenario results for the feature.
        /// </summary>
        public IEnumerable<IScenarioResult> Scenarios { get; }
    }
}
