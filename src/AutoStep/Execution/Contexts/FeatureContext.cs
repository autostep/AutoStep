using System;
using AutoStep.Elements.Metadata;

namespace AutoStep.Execution.Contexts
{
    /// <summary>
    /// The context object for a feature.
    /// </summary>
    public class FeatureContext : TestExecutionContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FeatureContext"/> class.
        /// </summary>
        /// <param name="feature">The feature metadata.</param>
        public FeatureContext(IFeatureInfo feature)
        {
            Feature = feature;
        }

        /// <summary>
        /// Gets the feature metadata.
        /// </summary>
        public IFeatureInfo Feature { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the feature ran (may be false if an event handler decided to skip it).
        /// </summary>
        public bool FeatureRan { get; set; }

        /// <summary>
        /// Gets or sets the exception that caused the entire feature to fail to start.
        /// This is not an exception from a test; it typically indicates an error in an
        /// event handler used before or after a feature.
        /// </summary>
        public Exception? FeatureFailureException { get; set; }
    }
}
