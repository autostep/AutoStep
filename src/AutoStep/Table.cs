using System;
using System.Collections.Generic;
using System.Text;
using AutoStep.Elements.Metadata;

namespace AutoStep
{
    public class Table
    {
        private readonly ITableInfo tableInfo;

        public Table(ITableInfo tableInfo)
        {
            this.tableInfo = tableInfo;
        }
    }
}
