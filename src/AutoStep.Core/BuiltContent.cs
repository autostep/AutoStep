using System.Collections.Generic;
using AutoStep.Core.Elements;

namespace AutoStep.Core
{
    /// <summary>
    /// Defines the built content of an AutoStep content source (i.e. a raw file becomes a set of built content).
    /// </summary>
    public class BuiltContent
    {
        /// <summary>
        /// Gets or sets any general step definitions defined in the file.
        /// </summary>
        public IEnumerable<BuiltStepDefinition> Steps { get; set; }

        /// <summary>
        /// Gets or sets the (optional feature) found in the file.
        /// </summary>
        public FeatureElement Feature { get; set; }
    }
}
