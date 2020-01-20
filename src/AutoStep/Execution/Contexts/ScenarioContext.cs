using AutoStep.Elements;
using AutoStep.Elements.ReadOnly;

namespace AutoStep.Execution
{

    public class ScenarioContext : ErrorCapturingContext
    {
        internal ScenarioContext(IScenarioInfo scenario, VariableSet example)
        {
            Scenario = scenario;
            Variables = example;
        }

        public IScenarioInfo Scenario { get; }

        public VariableSet Variables { get; }
    }
}
