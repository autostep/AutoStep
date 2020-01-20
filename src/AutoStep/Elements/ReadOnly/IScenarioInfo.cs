using System.Collections.Generic;

namespace AutoStep.Elements.ReadOnly
{
    public interface IScenarioInfo : IElementInfo, IStepCollectionInfo
    {
        IReadOnlyList<IAnnotationInfo> Annotations { get; }

        public string Name { get; }

        public string? Description { get; }
    }
}
