using System;
using AutoStep.Elements.Test;

namespace AutoStep.Tests.Builders
{
    public class TableBuilder : BaseBuilder<TableElement>
    {
        public TableBuilder(int line, int column)
        {
            Built = new TableElement
            {
                SourceLine = line,
                StartColumn = column
            };
        }

        public TableBuilder Headers(int lineNo, int column, params (string? headerName, int startColumn, int endColumn)[] headers)
        {
            Built.Header.SourceLine = lineNo;
            Built.Header.StartColumn = column;

            foreach(var item in headers)
            {
                Built.Header.AddHeader(new TableHeaderCellElement
                {
                    HeaderName = item.headerName,
                    SourceLine = lineNo,
                    StartColumn = item.startColumn,
                    EndColumn = item.endColumn,
                    EndLine = lineNo,
                });
            }

            return this;
        }

        public TableBuilder Row(int lineNo, int column, params (string? rawValue, int startColumn, int endColumn, Action<CellBuilder>? cfg)[] cells)
        {
            var row = new TableRowElement
            {
                SourceLine = lineNo,
                StartColumn = column
            };

            foreach(var item in cells)
            {
                var cell = new CellBuilder(item.rawValue, lineNo, item.startColumn, item.endColumn);

                if(item.cfg is object)
                {
                    item.cfg(cell);
                }
                else if(item.rawValue is string)
                {
                    cell.Text(item.rawValue);
                }

                row.AddCell(cell.Built);
            }

            Built.AddRow(row);

            return this;
        }
    }
}
