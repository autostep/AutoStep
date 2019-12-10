using System;
using AutoStep.Core;

namespace AutoStep.Compiler.Tests.Builders
{
    public class FeatureBuilder : BaseBuilder<BuiltFeature>
    {
        public FeatureBuilder(string name, int line, int column)
        {
            Built = new BuiltFeature
            {
                SourceLine = line,
                SourceColumn = column,
                Name = name
            };
        }

        public FeatureBuilder Tag(string tagName, int line, int column)
        {
            Built.Annotations.Add(new TagElement 
            { 
                SourceLine = line,
                SourceColumn = column,
                Tag = tagName
            });

            return this;
        }

        public FeatureBuilder Option(string optionName, int line, int column)
        {
            Built.Annotations.Add(new OptionElement 
            { 
                SourceLine = line,
                SourceColumn = column,
                Name = optionName 
            });

            return this;
        }

        public FeatureBuilder Option(string optionName, string setting, int line, int column)
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

        public FeatureBuilder Description(string description)
        {
            Built.Description = description;

            return this;
        }

        public FeatureBuilder Scenario(string name, int line, int column, Action<ScenarioBuilder> cfg)
        {
            var scenarioBuilder = new ScenarioBuilder(name, line, column);

            cfg(scenarioBuilder);

            Built.Scenarios.Add(scenarioBuilder.Built);

            return this;
        }
    }


}
