using System.Collections.Generic;

namespace AutoStep.Elements.Metadata
{
    /// <summary>
    /// Metadata for a table header.
    /// </summary>
    public interface ITableHeaderInfo : IElementInfo
    {
        /// <summary>
        /// Gets the set of all headers.
        /// </summary>
        IReadOnlyList<ITableHeaderCellInfo> Headers { get; }
    }
}
