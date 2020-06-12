using System;
using System.Collections.Generic;
using AutoStep.Elements.Metadata;

namespace AutoStep.Execution.Results
{
    /// <summary>
    /// Defines the structure for a single scenario result.
    /// </summary>
    public interface IScenarioResult
    {
        /// <summary>
        /// Gets the scenario information. Will be <see cref="IScenarioOutlineInfo"/> if <see cref="IsScenarioOutline"/> is true.
        /// </summary>
        public IScenarioInfo Scenario { get; }

        /// <summary>
        /// Gets a value indicating whether this scenario is an outline.
        /// </summary>
        public bool IsScenarioOutline { get; }

        /// <summary>
        /// Gets a value indicating whether all the invocations of this scenario have passed.
        /// </summary>
        public bool Passed { get; }

        /// <summary>
        /// Gets the set of all invocations of the scenario.
        /// </summary>
        public IReadOnlyList<IScenarioInvocationResult> Invocations { get; }
    }
}
