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
    }
}
