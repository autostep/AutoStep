using System.Collections.Generic;

namespace AutoStep.Elements.ReadOnly
{
    public interface IScenarioOutlineInfo : IScenarioInfo
    {
        IReadOnlyList<IExampleInfo> Examples { get; }
    }
}
