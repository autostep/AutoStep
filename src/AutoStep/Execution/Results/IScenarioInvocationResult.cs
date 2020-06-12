using System;
using AutoStep.Elements.Metadata;

namespace AutoStep.Execution.Results
{
    /// <summary>
    /// Defines the available result information for an individual invocation of a scenario.
    /// </summary>
    public interface IScenarioInvocationResult
    {
        /// <summary>
        /// Gets the scenario information. This may be an <see cref="IScenarioOutlineInfo"/> for scenario outlines.
        /// </summary>
        public IScenarioInfo Scenario { get; }

        /// <summary>
        /// Gets the individual name for the invocation. Will be null for regular scenarios, usually set for scenario outlines.
        /// </summary>
        public string? InvocationName { get; }

        /// <summary>
        /// Gets the set of variables provided to the scenario (if this is a scenario outline invocation).
        /// </summary>
        public TableVariableSet? OutlineVariables { get; }

        /// <summary>
        /// Gets a value indicating whether this scenario invocation passed.
        /// </summary>
        public bool Passed { get; }

        /// <summary>
        /// Gets the exception that caused test failure (if <see cref="Passed"/> is false).
        /// </summary>
        public Exception? FailException { get; }

        /// <summary>
        /// Gets the step that caused test failure (if <see cref="Passed"/> is false).
        /// </summary>
        public IStepReferenceInfo? FailingStep { get; }

        /// <summary>
        /// Gets the start time (UTC) of the scenario invocation.
        /// </summary>
        public DateTime StartTimeUtc { get; }

        /// <summary>
        /// Gets the end time (UTC) of the scenario invocation.
        /// </summary>
        public DateTime EndTimeUtc { get; }

        /// <summary>
        /// Gets the elapsed time take to execute the invocation.
        /// </summary>
        public TimeSpan Elapsed { get; }
    }
}
