using System;

namespace AutoStep.Definitions
{
    /// <summary>
    /// Represents a step definition source that can be updated while a project is 'active'.
    /// </summary>
    public interface IUpdatableStepDefinitionSource : IStepDefinitionSource
    {
        /// <summary>
        /// Gets the last modify time (UTC) of the source.
        /// </summary>
        /// <returns>The UTC timestamp indicating the moment at which the source was last changed.</returns>
        DateTime GetLastModifyTime();
    }
}
