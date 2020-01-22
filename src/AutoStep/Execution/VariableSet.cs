using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoStep.Elements;
using AutoStep.Elements.ReadOnly;
using AutoStep.Execution.Dependency;

namespace AutoStep.Execution
{
    public class VariableSet
    {
        public static readonly VariableSet Blank = new VariableSet();

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

        public static IEnumerable<VariableSet> CreateSetsForRows(ITableInfo table, IServiceScope scope, VariableSet currentVariables)
        {
            foreach (var row in table.Rows)
            {
                yield return Create(table, row, scope, currentVariables);
            }
        }

        private readonly Dictionary<string, string> valuesStore = new Dictionary<string, string>();

        public string Get(string variableName)
        {
            if (valuesStore.TryGetValue(variableName, out var varValue))
            {
                return varValue;
            }

            return string.Empty;
        }

        public void Set(string name, string value)
        {
            valuesStore[name] = value;
        }

        public IReadOnlyDictionary<string, string> Variables => valuesStore;
    }
}
