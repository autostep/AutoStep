using System;
using System.Collections.Generic;
using AutoStep.Elements.Metadata;
using AutoStep.Execution;

namespace AutoStep
{
    /// <summary>
    /// Defines a single row of a compiled table, as provided in <see cref="Table"/>.
    /// </summary>
    public class TableRow
    {
        private readonly IServiceProvider serviceProvider;
        private readonly ITableRowInfo srcRow;
        private readonly Dictionary<string, int> headers;
        private readonly VariableSet variables;

        private IReadOnlyList<string>? cellValues;

        /// <summary>
        /// Initializes a new instance of the <see cref="TableRow"/> class.
        /// </summary>
        /// <param name="srcRow">The source table row.</param>
        /// <param name="serviceProvider">A service provider.</param>
        /// <param name="headers">The look-up of header names to the position in the column set.</param>
        /// <param name="variables">The set of in-scope variables.</param>
        internal TableRow(ITableRowInfo srcRow, IServiceProvider serviceProvider, Dictionary<string, int> headers, VariableSet variables)
        {
            this.srcRow = srcRow;
            this.serviceProvider = serviceProvider;
            this.headers = headers;
            this.variables = variables;
        }

        /// <summary>
        /// Gets the set of all cells in the row.
        /// </summary>
        public IReadOnlyList<string> Cells => cellValues ??= BindCells();

        /// <summary>
        /// Gets a cell at the specified index in the row.
        /// </summary>
        /// <param name="cellIdx">The cell index.</param>
        /// <returns>The content of the cell.</returns>
        public string this[int cellIdx]
        {
            get => Cells[cellIdx];
        }

        /// <summary>
        /// Gets the cell value for the specified named column in this row.
        /// </summary>
        /// <param name="columnName">The name of the column.</param>
        /// <returns>The content of the cell.</returns>
        public string this[string columnName]
        {
            get
            {
                if (headers.TryGetValue(columnName, out var colIdx))
                {
                    return Cells[colIdx];
                }

                throw new ArgumentException(TableMessages.HeaderNotInTable.FormatWith(columnName));
            }
        }

        private IReadOnlyList<string> BindCells()
        {
            var result = new List<string>();

            foreach (var rawCell in srcRow.Cells)
            {
                // Bind any tokens in the cell.
                result.Add(rawCell.GetFullText(serviceProvider, variables));
            }

            return result;
        }
    }
}
