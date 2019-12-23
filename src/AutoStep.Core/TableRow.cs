using System;
using System.Collections.Generic;

namespace AutoStep.Core
{
    /// <summary>
    /// Represents a data-holding row in a table.
    /// </summary>
    public class TableRow : BuiltElement
    {
        private List<TableCell> cells = new List<TableCell>();

        /// <summary>
        /// Gets the set of cells in the row.
        /// </summary>
        public IReadOnlyList<TableCell> Cells => cells;

        /// <summary>
        /// Add a cell to the row.
        /// </summary>
        /// <param name="cell">The table cell to add.</param>
        public void AddCell(TableCell cell)
        {
            cells.Add(cell);
        }
    }
}
