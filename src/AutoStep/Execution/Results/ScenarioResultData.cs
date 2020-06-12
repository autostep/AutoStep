using System;
using System.Collections.Generic;
using System.Linq;
using AutoStep.Elements.Metadata;

namespace AutoStep.Execution.Results
{
    /// <summary>
    /// Stores the set of results for a given scenario (or outline).
    /// </summary>
    public class ScenarioResultData : IScenarioResult
    {
        private List<IScenarioInvocationResult> executions = new List<IScenarioInvocationResult>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ScenarioResultData"/> class.
        /// </summary>
        /// <param name="scenario">The scenario information.</param>
        public ScenarioResultData(IScenarioInfo scenario)
        {
            Scenario = scenario ?? throw new ArgumentNullException(nameof(scenario));
        }

        /// <inheritdoc/>
        public IScenarioInfo Scenario { get; }

        /// <inheritdoc/>
        public bool IsScenarioOutline => Scenario is IScenarioOutlineInfo;

        /// <inheritdoc/>
        public bool Passed => Invocations.All(x => x.Passed);

        /// <inheritdoc/>
        public IReadOnlyList<IScenarioInvocationResult> Invocations => executions;

        /// <summary>
        /// Add a new invocation to the scenario.
        /// </summary>
        /// <param name="startTimeUtc">The start time of the scenario invocation (in UTC).</param>
        /// <param name="invocationName">An optional name for the individual invocation of the scenario.</param>
        /// <param name="scenarioOutlineVariables">The set of variables passed into the scenario outline.</param>
        /// <returns>A new block of scenario invocation data, to update when the scenario finishes.</returns>
        public ScenarioInvocationResultData AddInvocation(DateTime startTimeUtc, string? invocationName, TableVariableSet? scenarioOutlineVariables)
        {
            var newData = new ScenarioInvocationResultData(Scenario, startTimeUtc, invocationName, scenarioOutlineVariables);

            executions.Add(newData);

            return newData;
        }
    }
}
