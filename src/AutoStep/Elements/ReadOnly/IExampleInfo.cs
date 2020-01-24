using System.Collections.Generic;

namespace AutoStep.Elements.Metadata
{
    /// <summary>
    /// Metada for an examples block in a scenario outline.
    /// </summary>
    public interface IExampleInfo : IElementInfo
    {
        /// <summary>
        /// Gets any annotations applied to the example.
        /// </summary>
        IReadOnlyList<IAnnotationInfo> Annotations { get; }

        /// <summary>
        /// Gets the table for the annotation.
        /// </summary>
        ITableInfo Table { get; }
    }
}
