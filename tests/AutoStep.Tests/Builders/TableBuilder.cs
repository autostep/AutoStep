using System;
using AutoStep;
using AutoStep.Elements;

namespace AutoStep.Tests.Builders
{
    public class TableBuilder : BaseBuilder<TableElement>
    {
        public TableBuilder(int line, int column)
        {
            Built = new TableElement
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
                Built.Header.AddHeader(new TableHeaderCellElement
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
            var row = new TableRowElement
            {
                SourceLine = lineNo,
                SourceColumn = column  
            };

            foreach(var item in cells)
            {
                var cell = new TableCellElement
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
