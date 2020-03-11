using System.Collections.Generic;

namespace AutoStep.Elements.Metadata
{
    /// <summary>
    /// Metadata for a table.
    /// </summary>
    public interface ITableInfo : IElementInfo
    {
        /// <summary>
        /// Gets the number of columns.
        /// </summary>
        int ColumnCount { get; }

        /// <summary>
        /// Gets the header row.
        /// </summary>
        ITableHeaderInfo Header { get; }

        /// <summary>
        /// Gets the set of data rows.
        /// </summary>
        IReadOnlyList<ITableRowInfo> Rows { get; }
    }
}
