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

        public FeatureBuilder Background(int line, int column, Action<BackgroundBuilder> cfg)
        {
            if(cfg is null)
            {
                throw new ArgumentNullException(nameof(cfg));
            }

            var backgroundBuilder = new BackgroundBuilder(line, column);

            cfg(backgroundBuilder);

            Built.Background = backgroundBuilder.Built;

            return this;
        }

        public FeatureBuilder Scenario(string name, int line, int column, Action<ScenarioBuilder> cfg = null)
        {
            var scenarioBuilder = new ScenarioBuilder(name, line, column);

            if (cfg is object)
            {
                cfg(scenarioBuilder);
            }

            Built.Scenarios.Add(scenarioBuilder.Built);

            return this;
        }
    }


}
