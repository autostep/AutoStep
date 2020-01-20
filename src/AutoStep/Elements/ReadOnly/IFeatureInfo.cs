using System.Collections.Generic;

namespace AutoStep.Elements.ReadOnly
{
    public interface IFeatureInfo : IElementInfo
    {
        IReadOnlyList<IAnnotationInfo> Annotations { get; }

        string Name { get; }

        string? Description { get; }

        IBackgroundInfo? Background { get; }

        IReadOnlyList<IScenarioInfo> Scenarios { get; }
    }
}
