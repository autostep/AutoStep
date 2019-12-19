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

        public ScenarioBuilder Tag(string tagName, int line, int column)
        {
            Built.Annotations.Add(new TagElement
            {
                SourceLine = line,
                SourceColumn = column,
                Tag = tagName
            });

            return this;
        }

        public ScenarioBuilder Option(string optionName, int line, int column)
        {
            Built.Annotations.Add(new OptionElement
            {
                SourceLine = line,
                SourceColumn = column,
                Name = optionName
            });

            return this;
        }
        public ScenarioBuilder Option(string optionName, string setting, int line, int column)
        {
            Built.Annotations.Add(new OptionElement
            {
                SourceLine = line,
                SourceColumn = column,
                Name = optionName,
                Setting = setting
            });

            return this;
        }


        public ScenarioBuilder Description(string description)
        {
            Built.Description = description;

            return this;
        }

    }


}
