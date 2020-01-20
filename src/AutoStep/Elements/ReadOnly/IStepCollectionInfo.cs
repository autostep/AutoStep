using System.Collections.Generic;

namespace AutoStep.Elements.ReadOnly
{
    public interface IStepCollectionInfo : IElementInfo
    {
        IReadOnlyList<IStepReferenceInfo> Steps { get; }
    }
}
