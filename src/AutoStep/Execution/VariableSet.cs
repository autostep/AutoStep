using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoStep.Elements;
using AutoStep.Elements.ReadOnly;

namespace AutoStep.Execution
{
    public class VariableSet
    {
        public static readonly VariableSet Blank = new VariableSet();

        public static VariableSet Create(ITableInfo table, ITableRowInfo row)
        {
            throw new NotImplementedException();
        }

        public static IEnumerable<VariableSet> CreateSet(ITableInfo table)
        {
            foreach (var row in table.Rows)
            {
                yield return Create(table, row);
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
    }
}
