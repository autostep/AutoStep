using System.Collections.Generic;

namespace AutoStep.Elements.Metadata
{
    /// <summary>
    /// Metadata for a collection of step references.
    /// </summary>
    public interface IStepCollectionInfo : IElementInfo
    {
        /// <summary>
        /// Gets the set of steps in the collection.
        /// </summary>
        IReadOnlyList<IStepReferenceInfo> Steps { get; }
    }
}
