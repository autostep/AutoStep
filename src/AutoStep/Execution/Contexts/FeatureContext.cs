using AutoStep.Elements.ReadOnly;

namespace AutoStep.Execution
{

    public class FeatureContext : TestExecutionContext
    {
        internal FeatureContext(IFeatureInfo feature)
        {
            Feature = feature;
        }

        public IFeatureInfo Feature { get; }
    }
}
