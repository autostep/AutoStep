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

        public FileBuilder Feature(string featureName, Action<FeatureBuilder> cfg)
        {
            if(Built.Feature != null)
            {
                throw new InvalidOperationException("Cannot have more than one feature in a file.");
            }

            var featureBuilder = new FeatureBuilder(featureName);
            cfg(featureBuilder);

            Built.Feature = featureBuilder.Built;

            return this;
        }
    }

    public class FeatureBuilder : BaseBuilder<BuiltFeature>
    {
        public FeatureBuilder(string name)
        {
            Built = new BuiltFeature
            {
                Name = name
            };
        }            

        public FeatureBuilder Tag(string tagName)
        {
            Built.Annotations.Add(new TagElement { Tag = tagName });

            return this;
        }
        public FeatureBuilder Option(string optionName)
        {
            Built.Annotations.Add(new OptionElement { Name = optionName });

            return this;
        }

        public FeatureBuilder Description(string description)
        {
            Built.Description = description;

            return this;
        }

        public FeatureBuilder Scenario(string name, Action<ScenarioBuilder> cfg)
        {
            var scenarioBuilder = new ScenarioBuilder(name);

            cfg(scenarioBuilder);

            Built.Scenarios.Add(scenarioBuilder.Built);

            return this;
        }
    }

    public class ScenarioBuilder : BaseBuilder<BuiltScenario>
    {
        public ScenarioBuilder(string name)
        {
            Built = new BuiltScenario
            {
                Name = name
            };
        }

        public ScenarioBuilder Tag(string tagName)
        {
            Built.Annotations.Add(new TagElement { Tag = tagName });

            return this;
        }
        public ScenarioBuilder Option(string optionName)
        {
            Built.Annotations.Add(new OptionElement { Name = optionName });

            return this;
        }

        public ScenarioBuilder Description(string description)
        {
            Built.Description = description;

            return this;
        }

        public ScenarioBuilder Given(string body)
        {
            Built.Steps.Add(new UnknownStepReference { Type = StepType.Given, Text = body });

            return this;
        }

        public ScenarioBuilder When(string body)
        {
            Built.Steps.Add(new UnknownStepReference { Type = StepType.When, Text = body });

            return this;
        }

        public ScenarioBuilder Then(string body)
        {
            Built.Steps.Add(new UnknownStepReference { Type = StepType.Then, Text = body });

            return this;
        }

        public ScenarioBuilder And(string body)
        {
            Built.Steps.Add(new UnknownStepReference { Type = StepType.And, Text = body });

            return this;
        }
    }


}
