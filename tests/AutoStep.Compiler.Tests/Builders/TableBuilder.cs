using System;
using AutoStep.Core;

namespace AutoStep.Compiler.Tests.Builders
{
    public class TableBuilder : BaseBuilder<BuiltTable>
    {
        public TableBuilder(int line, int column)
        {
            Built = new BuiltTable
            {
                SourceLine = line,
                SourceColumn = column
            };
        }

        public TableBuilder Headers(int lineNo, int column, params (string headerName, int startColumn, int endColumn)[] headers)
        {
            Built.Header.SourceLine = lineNo;
            Built.Header.SourceColumn = column;

            foreach(var item in headers)
            {
                Built.Header.AddHeader(new TableHeaderCell
                {
                    HeaderName = item.headerName,
                    SourceLine = lineNo,
                    SourceColumn = item.startColumn,
                    EndColumn = item.endColumn
                });
            }

            return this;
        }

        public TableBuilder Row(int lineNo, int column, params (ArgumentType argType, string rawValue, int startColumn, int endColumn, Action<ArgumentBuilder> cfg)[] cells)
        {
            var row = new TableRow
            {
                SourceLine = lineNo,
                SourceColumn = column  
            };

            foreach(var item in cells)
            {
                var cell = new TableCell
                {
                    SourceLine = lineNo,
                    SourceColumn = item.startColumn,
                    EndColumn = item.endColumn
                };

                var argument = new ArgumentBuilder(cell, item.rawValue, item.argType, item.startColumn, item.endColumn);

                if(item.cfg is object)
                {
                    item.cfg(argument);
                }

                cell.Value = argument.Built;

                row.AddCell(cell);
            }

            Built.AddRow(row);

            return this;
        }
    }
}
