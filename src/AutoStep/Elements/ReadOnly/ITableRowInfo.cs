using System.Collections.Generic;

namespace AutoStep.Elements.ReadOnly
{
    public interface ITableRowInfo : IElementInfo
    {
        IReadOnlyList<ITableCellInfo> Cells { get; }
    }
}
