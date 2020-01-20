using System.Collections.Generic;

namespace AutoStep.Elements.ReadOnly
{
    public interface IExampleInfo : IElementInfo
    {
        IReadOnlyList<IAnnotationInfo> Annotations { get; }

        ITableInfo Table { get; }
    }
}
