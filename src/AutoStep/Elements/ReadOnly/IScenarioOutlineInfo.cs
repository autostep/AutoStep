using System.Collections.Generic;

namespace AutoStep.Elements.Metadata
{
    /// <summary>
    /// Metadata for a scenario outline.
    /// </summary>
    public interface IScenarioOutlineInfo : IScenarioInfo
    {
        /// <summary>
        /// Gets the set of examples for the scenario outline.
        /// </summary>
        IReadOnlyList<IExampleInfo> Examples { get; }
    }
}
