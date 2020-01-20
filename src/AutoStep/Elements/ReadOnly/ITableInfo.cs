using System.Collections.Generic;

namespace AutoStep.Elements.ReadOnly
{
    public interface ITableInfo : IElementInfo
    {
        int ColumnCount { get; }

        ITableHeaderInfo Header { get; }

        IReadOnlyList<ITableRowInfo> Rows { get; }
    }
}
