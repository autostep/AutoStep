using System;
using System.Collections.Generic;
using AutoStep.Elements.Metadata;
using AutoStep.Execution.Dependency;

namespace AutoStep.Execution
{
    /// <summary>
    /// This class holds a set of variables used for resolving variable references in steps.
    /// </summary>
    public class VariableSet
    {
        private readonly Dictionary<string, string> valuesStore = new Dictionary<string, string>();
        private readonly bool isReadOnly;

        /// <summary>
        /// Initializes a new instance of the <see cref="VariableSet"/> class.
        /// </summary>
        /// <param name="isReadOnly">Set to true to prevent modification of this set after it has been created.</param>
        public VariableSet(bool isReadOnly = false)
        {
            this.isReadOnly = isReadOnly;
        }

        /// <summary>
        /// Gets the set of known variables.
        /// </summary>
        public IReadOnlyDictionary<string, string> Variables => valuesStore;

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

        /// <summary>
        /// Get the value of a variable. If the variable is not set, an empty string will be returned.
        /// </summary>
        /// <param name="variableName">The name of a variable.</param>
        /// <returns>The value of the variable.</returns>
        public string Get(string variableName)
        {
            if (valuesStore.TryGetValue(variableName, out var varValue))
            {
                return varValue;
            }

            return string.Empty;
        }

        /// <summary>
        /// Set a value in the variable set.
        /// </summary>
        /// <param name="name">The name of the variable.</param>
        /// <param name="value">The value to set.</param>
        public void Set(string name, string value)
        {
            if (isReadOnly)
            {
                throw new InvalidOperationException(ExecutionText.VariableSet_ReadOnly);
            }

            valuesStore[name] = value;
        }
    }
}
