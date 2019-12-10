using System;
using AutoStep.Core;

namespace AutoStep.Compiler.Tests.Builders
{

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


}
