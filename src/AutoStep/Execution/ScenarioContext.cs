using AutoStep.Elements;

namespace AutoStep.Execution
{

    public class ScenarioContext : ErrorCapturingContext
    {
        private ExampleElement? example;

        internal ScenarioContext(ScenarioElement scenario, VariableSet example)
        {
            Scenario = scenario;
            Variables = example;
        }

        public ScenarioElement Scenario { get; }

        public VariableSet Variables { get; }
    }
}
