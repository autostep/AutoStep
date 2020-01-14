using System;
using AutoStep.Elements;

namespace AutoStep.Tests.Builders
{
    public class FeatureBuilder : BaseBuilder<FeatureElement>
    {
        public FeatureBuilder(string name, int line, int column, bool relativeToTextContent = false)
            : base(relativeToTextContent)
        {
            Built = new FeatureElement
            {
                SourceLine = line,
                SourceColumn = column,
                Name = name
            };
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

            var backgroundBuilder = new BackgroundBuilder(line, column, RelativeToTextContent);

            cfg(backgroundBuilder);

            Built.Background = backgroundBuilder.Built;

            return this;
        }

        public FeatureBuilder Scenario(string name, int line, int column, Action<ScenarioBuilder> cfg = null)
        {
            var scenarioBuilder = new ScenarioBuilder(name, line, column, RelativeToTextContent);

            if (cfg is object)
            {
                cfg(scenarioBuilder);
            }

            Built.Scenarios.Add(scenarioBuilder.Built);

            return this;
        }

        public FeatureBuilder ScenarioOutline(string name, int line, int column, Action<ScenarioOutlineBuilder> cfg = null)
        {
            var scenarioOutlineBuilder = new ScenarioOutlineBuilder(name, line, column, RelativeToTextContent);

            if(cfg is object)
            {
                cfg(scenarioOutlineBuilder);
            }

            Built.Scenarios.Add(scenarioOutlineBuilder.Built);

            return this;
        }
    }


}
