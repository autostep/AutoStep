using System.Collections.Generic;
using AutoStep.Core;

namespace AutoStep.Compiler.Tests.Builders
{

    public class ScenarioBuilder : BaseBuilder<BuiltScenario>, IStepCollectionBuilder<BuiltScenario>
    {
        public ScenarioBuilder(string name, int line, int column)
        {
            Built = new BuiltScenario
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
