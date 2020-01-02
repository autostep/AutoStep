using System.Collections.Generic;
using AutoStep.Core.Elements;

namespace AutoStep.Compiler.Tests.Builders
{

    public class ScenarioBuilder : BaseBuilder<ScenarioElement>, IStepCollectionBuilder<ScenarioElement>
    {
        public ScenarioBuilder(string name, int line, int column)
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
