using System.Collections.Generic;
using AutoStep.Elements.Metadata;
using AutoStep.Execution.Dependency;

namespace AutoStep.Execution
{
    /// <summary>
    /// This class holds a set of variables used for resolving variable references in steps.
    /// </summary>
    public class VariableSet : VariableSetBase<string>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VariableSet"/> class.
        /// </summary>
        /// <param name="isReadOnly">Set to true to prevent modification of this set after it has been created.</param>
        public VariableSet(bool isReadOnly = false)
            : base(isReadOnly)
        {
        }

        protected override string? GetDefault()
        {
            return string.Empty;
        }

        /// <summary>
        /// Gets an fixed empty variable set.
        /// </summary>
        public static VariableSet Blank { get; } = new VariableSet(true);

        /// <summary>
        /// Create a variable set from a row in a table.
        /// </summary>
        /// <param name="table">The table metadata.</param>
        /// <param name="row">The row metadata.</param>
        /// <param name="scope">The current execution scope.</param>
        /// <param name="currentVariables">The variables currently in scope.</param>
        /// <returns>A new variable set, with named values for each column.</returns>
        public static VariableSet Create(ITableInfo table, ITableRowInfo row, IServiceScope scope, VariableSet currentVariables)
        {
            table = table.ThrowIfNull(nameof(table));
            row = row.ThrowIfNull(nameof(row));

            var set = new VariableSet();

            for (int idx = 0; idx < table.ColumnCount; idx++)
            {
                if (!string.IsNullOrWhiteSpace(table.Header.Headers[idx].HeaderName))
                {
                    set.Set(table.Header.Headers[idx].HeaderName!, row.Cells[idx].GetFullText(scope, currentVariables));
                }
            }

            return set;
        }

        /// <summary>
        /// Create an enumerable list of variable sets from each row in a table.
        /// </summary>
        /// <param name="table">The table metadata.</param>
        /// <param name="scope">The current execution scope.</param>
        /// <param name="currentVariables">The variables currently in scope.</param>
        /// <returns>A new variable set, with named values for each column.</returns>
        public static IEnumerable<VariableSet> CreateSetsForRows(ITableInfo table, IServiceScope scope, VariableSet currentVariables)
        {
            table = table.ThrowIfNull(nameof(table));

            foreach (var row in table.Rows)
            {
                yield return Create(table, row, scope, currentVariables);
            }
        }
    }
}
