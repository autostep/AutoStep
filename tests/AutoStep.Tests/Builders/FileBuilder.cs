using System;
using System.Linq;
using AutoStep.Elements.Test;

namespace AutoStep.Tests.Builders
{
    public class FileBuilder : BaseBuilder<FileElement>
    {
        public FileBuilder()
        {
            Built = new FileElement();
        }

        public FileBuilder Feature(string featureName, int line, int column, Action<FeatureBuilder>? cfg = null)
        {
            if(Built.Feature != null)
            {
                throw new InvalidOperationException("Cannot have more than one feature in a file.");
            }

            var featureBuilder = new FeatureBuilder(featureName, line, column);

            if (cfg != null)
            {
                cfg(featureBuilder);
            }

            Built.Feature = featureBuilder.Built;

            if(Built.Feature.Background is object)
            {
                foreach (var step in Built.Feature.Background.Steps)
                {
                    Built.AllStepReferences.AddLast(step);
                }
            }

            // Go through all the scenarios and steps and add them.
            foreach (var step in Built.Feature.Scenarios.SelectMany(x => x.Steps))
            {
                Built.AllStepReferences.AddLast(step);
            }

            return this;
        }

        public FileBuilder StepDefinition(StepType type, string declaration, int line, int column, Action<StepDefinitionBuilder>? cfg = null)
        {
            var stepDefinitionBuilder = new StepDefinitionBuilder(type, declaration, line, column);

            if (cfg is object)
            {
                cfg(stepDefinitionBuilder);
            }

            Built.AddStepDefinition(stepDefinitionBuilder.Built);

            // Go through all the scenarios and steps and add them.
            foreach (var step in stepDefinitionBuilder.Built.Steps)
            {
                Built.AllStepReferences.AddLast(step);
            }

            return this;
        }
    }
}
