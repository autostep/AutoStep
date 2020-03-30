using AutoStep.Elements.Test;

namespace AutoStep.Tests.Builders
{
    public class ScenarioBuilder : BaseBuilder<ScenarioElement>, IStepCollectionBuilder<ScenarioElement>
    {
        public ScenarioBuilder(string name, int line, int column)
        {
            Built = new ScenarioElement
            {
                SourceLine = line,
                StartColumn = column,
                Name = name,
                EndLine = line,
                EndColumn = (column + ("Scenario: ".Length) + name.Length) - 1
            };
        }

        public ScenarioBuilder Description(string description)
        {
            Built.Description = description;

            return this;
        }
    }
}
