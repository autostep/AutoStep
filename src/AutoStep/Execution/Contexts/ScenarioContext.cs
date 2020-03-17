using AutoStep.Elements.Metadata;

namespace AutoStep.Execution.Contexts
{
    /// <summary>
    /// Context type for a scenario.
    /// </summary>
    public class ScenarioContext : StepCollectionContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ScenarioContext"/> class.
        /// </summary>
        /// <param name="scenario">The scenario metadata.</param>
        /// <param name="variables">The set of variables that currently apply to the scenario.</param>
        internal ScenarioContext(IScenarioInfo scenario, VariableSet variables)
        {
            Scenario = scenario;
            Variables = variables;
        }

        /// <summary>
        /// Gets the metadata for the scenario.
        /// </summary>
        public IScenarioInfo Scenario { get; }

        /// <summary>
        /// Gets the set of variables that apply to the current scenario.
        /// </summary>
        public VariableSet Variables { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the scenario ran (may be false if an event handler decided to skip it).
        /// </summary>
        public bool ScenarioRan { get; set; }
    }
}
