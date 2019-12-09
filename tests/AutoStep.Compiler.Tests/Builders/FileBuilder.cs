using System;
using AutoStep.Core;

namespace AutoStep.Compiler.Tests.Builders
{
    public class BaseBuilder<TBuiltComponent>
    {
        public TBuiltComponent Built { get; set; }
    }

    public class FileBuilder : BaseBuilder<BuiltFile>
    {
        public FileBuilder()
        {
            Built = new BuiltFile();
        }

        public FileBuilder Feature(string featureName, int line, int column, Action<FeatureBuilder> cfg)
        {
            if(Built.Feature != null)
            {
                throw new InvalidOperationException("Cannot have more than one feature in a file.");
            }

            var featureBuilder = new FeatureBuilder(featureName, line, column);
            cfg(featureBuilder);

            Built.Feature = featureBuilder.Built;

            return this;
        }
    }

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

    public class ScenarioBuilder : BaseBuilder<BuiltScenario>
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

        public ScenarioBuilder Description(string description)
        {
            Built.Description = description;

            return this;
        }

        public ScenarioBuilder Given(string body, int line, int column)
        {
            Built.Steps.Add(new UnknownStepReference 
            { 
                SourceLine = line,
                SourceColumn = column,
                Type = StepType.Given, 
                Text = body 
            });

            return this;
        }

        public ScenarioBuilder When(string body, int line, int column)
        {
            Built.Steps.Add(new UnknownStepReference 
            { 
                SourceLine = line,
                SourceColumn = column,
                Type = StepType.When,
                Text = body
            });

            return this;
        }

        public ScenarioBuilder Then(string body, int line, int column)
        {
            Built.Steps.Add(new UnknownStepReference
            {
                SourceLine = line,
                SourceColumn = column,
                Type = StepType.Then,
                Text = body
            });

            return this;
        }

        public ScenarioBuilder And(string body, int line, int column)
        {
            Built.Steps.Add(new UnknownStepReference 
            { 
                SourceLine = line,
                SourceColumn = column,
                Type = StepType.And,
                Text = body
            });

            return this;
        }
    }


}
