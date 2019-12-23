using System;
using System.Collections.Generic;

namespace AutoStep.Core
{
    public class BuiltTable : BuiltElement
    {
        private List<TableRow> rows = new List<TableRow>();

        public TableHeader Header { get; set; }

        public int ColumnCount => Header?.Headers.Count ?? 0;

        public IReadOnlyList<TableRow> Rows => rows;

        public void AddRow(TableRow row)
        {
            rows.Add(row);
        }
    }

    public class TableHeader : BuiltElement
    {
        private List<TableHeaderCell> headers = new List<TableHeaderCell>();

        public IReadOnlyList<TableHeaderCell> Headers => headers;

        public void AddHeader(TableHeaderCell header)
        {
            headers.Add(header);
        }
    }

    public class TableHeaderCell : PositionalElement
    {
        public string HeaderName { get; set; }
    }

    public class TableRow : BuiltElement
    {
        private List<TableCell> cells = new List<TableCell>();

        public IReadOnlyList<TableCell> Cells => cells;

        public void AddCell(TableCell cell)
        {
            cells.Add(cell);
        }
    }

    public class TableCell : PositionalElement
    {
        public StepArgument Value { get; set; }
    }
}
