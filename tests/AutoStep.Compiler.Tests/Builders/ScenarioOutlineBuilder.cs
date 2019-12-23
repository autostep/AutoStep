using System;
using System.Collections.Generic;
using AutoStep.Core;

namespace AutoStep.Compiler.Tests.Builders
{

    public class ScenarioOutlineBuilder : BaseBuilder<BuiltScenarioOutline>, IStepCollectionBuilder<BuiltScenarioOutline>
    {
        public ScenarioOutlineBuilder(string name, int line, int column)
        {
            Built = new BuiltScenarioOutline
            {
                SourceLine = line,
                SourceColumn = column,
                Name = name
            };
        }
        
        public ScenarioOutlineBuilder Description(string description)
        {
            Built.Description = description;

            return this;
        }

        public ScenarioOutlineBuilder Examples(int line, int column, Action<ExampleBuilder> cfg)
        {
            var newExample = new ExampleBuilder(line, column);

            cfg(newExample);

            Built.Examples.Add(newExample.Built);

            return this;
        }
    }


}
