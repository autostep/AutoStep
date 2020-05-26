using System.Collections.Generic;

namespace AutoStep.Elements.Metadata
{
    /// <summary>
    /// Metadata for a Feature.
    /// </summary>
    public interface IFeatureInfo : IElementInfo
    {
        /// <summary>
        /// Gets the set of annotations applied to the feature.
        /// </summary>
        IReadOnlyList<IAnnotationInfo> Annotations { get; }

        /// <summary>
        /// Gets the name of the containing source file.
        /// </summary>
        string? SourceName { get; }

        /// <summary>
        /// Gets the name of the feature.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the optional feature description.
        /// </summary>
        string? Description { get; }

        /// <summary>
        /// Gets the optional background block for the feature.
        /// </summary>
        IBackgroundInfo? Background { get; }

        /// <summary>
        /// Gets the set of scenarios (and scenario outlines) in the feature.
        /// </summary>
        IReadOnlyList<IScenarioInfo> Scenarios { get; }
    }
}
