using System;
using AutoStep.Elements.Metadata;
using AutoStep.Execution.Contexts;

namespace AutoStep.Execution.Results
{
    /// <summary>
    /// Stores the result data for a single scenario invocation.
    /// </summary>
    public class ScenarioInvocationResultData : IScenarioInvocationResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ScenarioInvocationResultData"/> class.
        /// </summary>
        /// <param name="scenario">The scenario info.</param>
        /// <param name="startTimeUtc">The start time of the scenario invocation (in UTC).</param>
        /// <param name="invocationName">An optional name for the individual invocation of the scenario.</param>
        /// <param name="scenarioOutlineVariables">The set of variables passed into the scenario outline.</param>
        public ScenarioInvocationResultData(IScenarioInfo scenario, DateTime startTimeUtc, string? invocationName, TableVariableSet? scenarioOutlineVariables)
        {
            Scenario = scenario ?? throw new ArgumentNullException(nameof(scenario));
            InvocationName = invocationName;
            OutlineVariables = scenarioOutlineVariables;
            StartTimeUtc = startTimeUtc;
        }

        /// <inheritdoc/>
        public IScenarioInfo Scenario { get; }

        /// <inheritdoc/>
        public string? InvocationName { get; }

        /// <inheritdoc/>
        public TableVariableSet? OutlineVariables { get; }

        /// <inheritdoc/>
        public bool Passed { get; private set; }

        /// <inheritdoc/>
        public DateTime StartTimeUtc { get; }

        /// <inheritdoc/>
        public Exception? FailException { get; private set; }

        /// <inheritdoc/>
        public IStepReferenceInfo? FailingStep { get; private set; }

        /// <inheritdoc/>
        public DateTime EndTimeUtc { get; private set; }

        /// <inheritdoc/>
        public TimeSpan Elapsed { get; private set; }

        /// <summary>
        /// Update the outcome of the scenario invocation from an execution-time scenario context.
        /// </summary>
        /// <param name="endTimeUtc">The end time of the invocation (UTC).</param>
        /// <param name="invokeElapsed">The elapsed time of the invocation.</param>
        /// <param name="failException">The failure exception (if the step failed).</param>
        /// <param name="failingStep">The step that caused the scenario to fail (if it failed).</param>
        public void UpdateOutcome(DateTime endTimeUtc, TimeSpan invokeElapsed, Exception? failException, IStepReferenceInfo? failingStep)
        {
            EndTimeUtc = endTimeUtc;
            Elapsed = invokeElapsed;

            if (failException is null)
            {
                Passed = true;
                FailException = null;
                FailingStep = null;
            }
            else
            {
                Passed = false;
                FailException = failException;
                FailingStep = failingStep;
            }
        }
    }
}
