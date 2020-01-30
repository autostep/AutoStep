using System.Collections.Generic;
using AutoStep.Elements.Metadata;

namespace AutoStep.Elements
{
    /// <summary>
    /// Represents a data-holding row in a table.
    /// </summary>
    public class TableRowElement : BuiltElement, ITableRowInfo
    {
        private readonly List<TableCellElement> cells = new List<TableCellElement>();

        /// <summary>
        /// Gets the set of cells in the row.
        /// </summary>
        public IReadOnlyList<TableCellElement> Cells => cells;

        /// <inheritdoc/>
        IReadOnlyList<ITableCellInfo> ITableRowInfo.Cells => cells;

        /// <summary>
        /// Add a cell to the row.
        /// </summary>
        /// <param name="cell">The table cell to add.</param>
        public void AddCell(TableCellElement cell)
        {
            cells.Add(cell);
        }
    }
}
