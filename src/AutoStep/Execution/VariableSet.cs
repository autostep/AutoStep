using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoStep.Elements;

namespace AutoStep.Execution
{
    public class VariableSet
    {
        public static VariableSet Create(TableElement table, TableRowElement row)
        {
            throw new NotImplementedException();
        }

        public static IEnumerable<VariableSet> CreateSet(TableElement table)
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
