using System.Collections.Generic;

namespace AutoStep.Elements
{
    /// <summary>
    /// Represents a data-holding row in a table.
    /// </summary>
    public class TableRowElement : BuiltElement
    {
        private List<TableCellElement> cells = new List<TableCellElement>();

        /// <summary>
        /// Gets the set of cells in the row.
        /// </summary>
        public IReadOnlyList<TableCellElement> Cells => cells;

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
