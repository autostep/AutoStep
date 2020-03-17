using AutoStep.Elements.Test;

namespace AutoStep.Tests.Builders
{
    public class ScenarioBuilder : BaseBuilder<ScenarioElement>, IStepCollectionBuilder<ScenarioElement>
    {
        public ScenarioBuilder(string name, int line, int column) : base(
            new ScenarioElement
        {
            SourceLine = line,
            StartColumn = column,
            Name = name
        })
        {
        }

        public ScenarioBuilder Description(string description)
        {
            Built.Description = description;

            return this;
        }
    }
}
