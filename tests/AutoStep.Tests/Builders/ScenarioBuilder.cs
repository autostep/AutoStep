using System.Collections.Generic;
using AutoStep.Elements;

namespace AutoStep.Tests.Builders
{

    public class ScenarioBuilder : BaseBuilder<ScenarioElement>, IStepCollectionBuilder<ScenarioElement>
    {
        public ScenarioBuilder(string name, int line, int column, bool relativeToTextContent = false)
            : base(relativeToTextContent)
        {
            Built = new ScenarioElement
            {
                SourceLine = line,
                SourceColumn = column,
                Name = name
            };
        }
        
        public ScenarioBuilder Description(string description)
        {
            Built.Description = description;

            return this;
        }
    }


}
