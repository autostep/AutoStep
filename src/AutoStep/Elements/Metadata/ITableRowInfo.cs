using System.Collections.Generic;

namespace AutoStep.Elements.Metadata
{
    /// <summary>
    /// Metadata for a table row.
    /// </summary>
    public interface ITableRowInfo : IElementInfo
    {
        /// <summary>
        /// Gets the set of all cells in the row.
        /// </summary>
        IReadOnlyList<ITableCellInfo> Cells { get; }
    }
}
