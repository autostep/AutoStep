using System.Collections.Generic;

namespace AutoStep.Core
{
    /// <summary>
    /// Defines a built Feature block, that can contain Background and Scenarios.
    /// </summary>
    public class BuiltFeature : BuiltElement, IAnnotatable
    {
        /// <summary>
        /// Gets the annotations applied to the feature, in applied order.
        /// </summary>
        public List<AnnotationElement> Annotations { get; } = new List<AnnotationElement>();

        /// <summary>
        /// Gets or sets the name of the feature.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description body (if any).
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the background set, or null if none is specified.
        /// </summary>
        public BuiltBackground Background { get; set; }

        /// <summary>
        /// Gets the list of scenarios.
        /// </summary>
        public List<BuiltScenario> Scenarios { get; } = new List<BuiltScenario>();
    }
}
