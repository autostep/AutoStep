using System.Collections.Generic;

namespace AutoStep.Core
{
    /// <summary>
    /// Represents a table built from autostep source.
    /// </summary>
    public class BuiltTable : BuiltElement
    {
        private List<TableRow> rows = new List<TableRow>();

        /// <summary>
        /// Gets the header row.
        /// </summary>
        public TableHeader Header { get; } = new TableHeader();

        /// <summary>
        /// Gets the number of columns expected in the table (based on the number of headers).
        /// </summary>
        public int ColumnCount => Header?.Headers.Count ?? 0;

        /// <summary>
        /// Gets the non-header rows in the table.
        /// </summary>
        public IReadOnlyList<TableRow> Rows => rows;

        /// <summary>
        /// Adds a row to the table.
        /// </summary>
        /// <param name="row">The row.</param>
        public void AddRow(TableRow row)
        {
            rows.Add(row);
        }
    }
}
