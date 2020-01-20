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

        public static VariableSet CreateBlank()
        {
            return new VariableSet();
        }

        public string GetVariableText(string variableName)
        {
            throw new NotImplementedException();
        }
    }
}
