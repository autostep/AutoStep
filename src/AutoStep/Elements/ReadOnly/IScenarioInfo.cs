using System.Collections.Generic;

namespace AutoStep.Elements.Metadata
{
    /// <summary>
    /// Metadata for an executing Scenario.
    /// </summary>
    public interface IScenarioInfo : IElementInfo, IStepCollectionInfo
    {
        /// <summary>
        /// Gets the set of annotations applied to the Scenario.
        /// </summary>
        IReadOnlyList<IAnnotationInfo> Annotations { get; }

        /// <summary>
        /// Gets the name of the Scenario.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the optional Scenario Description.
        /// </summary>
        public string? Description { get; }
    }
}
