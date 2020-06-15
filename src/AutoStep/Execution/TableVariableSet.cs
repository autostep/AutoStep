using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using AutoStep.Elements.Metadata;

namespace AutoStep.Execution
{
    /// <summary>
    /// Defines a variable set backed by a row in a table. Maintains metadata about the source table and row.
    /// </summary>
    public class TableVariableSet : VariableSet
    {
        /// <summary>
        /// Create a variable set from a row in a table.
        /// </summary>
        /// <param name="table">The table metadata.</param>
        /// <param name="row">The row metadata.</param>
        /// <param name="scope">The current execution scope.</param>
        /// <param name="currentVariables">The variables currently in scope.</param>
        /// <returns>A new variable set, with named values for each column.</returns>
        public static TableVariableSet Create(ITableInfo table, ITableRowInfo row, ILifetimeScope scope, VariableSet currentVariables)
        {
            table = table.ThrowIfNull(nameof(table));
            row = row.ThrowIfNull(nameof(row));

            var set = new TableVariableSet(table, row);

            set.LoadVariables(scope, currentVariables);

            return set;
        }

        /// <summary>
        /// Create an enumerable list of variable sets from each row in a table.
        /// </summary>
        /// <param name="table">The table metadata.</param>
        /// <param name="scope">The current execution scope.</param>
        /// <param name="currentVariables">The variables currently in scope.</param>
        /// <returns>A new variable set, with named values for each column.</returns>
        public static IEnumerable<TableVariableSet> CreateSetsForRows(ITableInfo table, ILifetimeScope scope, VariableSet currentVariables)
        {
            table = table.ThrowIfNull(nameof(table));

            foreach (var row in table.Rows)
            {
                yield return Create(table, row, scope, currentVariables);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TableVariableSet"/> class.
        /// </summary>
        /// <param name="tableInfo">The table metadata.</param>
        /// <param name="rowInfo">The row metadata.</param>
        public TableVariableSet(ITableInfo tableInfo, ITableRowInfo rowInfo)
        {
            Table = tableInfo;
            Row = rowInfo;
        }

        /// <summary>
        /// Gets the set of non-null column names, in column order.
        /// </summary>
        public IEnumerable<string> ColumnNames => Table.Header.Headers.Where(x => !string.IsNullOrWhiteSpace(x.HeaderName)).Select(x => x.HeaderName)!;

        /// <summary>
        /// Gets the table metadata this set was created from.
        /// </summary>
        public ITableInfo Table { get; }

        /// <summary>
        /// Gets the row metadata this set was created from.
        /// </summary>
        public ITableRowInfo Row { get; }

        /// <summary>
        /// Load (or re-load) the variables in the set from the provided metadata.
        /// </summary>
        /// <param name="scope">A service scope for resolving services.</param>
        /// <param name="scopedVariables">The set of in-scope variables that will be used to resolve any variable references in the table row.</param>
        public void LoadVariables(ILifetimeScope scope, VariableSet scopedVariables)
        {
            var headers = Table.Header.Headers;

            for (int idx = 0; idx < Table.ColumnCount; idx++)
            {
                if (!string.IsNullOrWhiteSpace(headers[idx].HeaderName))
                {
                    Set(headers[idx].HeaderName!, Row.Cells[idx].GetFullText(scope, scopedVariables));
                }
            }
        }
    }
}
