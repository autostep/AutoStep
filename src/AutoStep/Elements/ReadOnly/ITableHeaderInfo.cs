using System.Collections.Generic;

namespace AutoStep.Elements.ReadOnly
{
    public interface ITableHeaderInfo : IElementInfo
    {
        IReadOnlyList<ITableHeaderCellInfo> Headers { get; }
    }
}
