using AutoStep.Elements.ReadOnly;

namespace AutoStep.Execution
{

    public class FeatureContext : ExecutionContext
    {
        internal FeatureContext(IFeatureInfo feature)
        {
            Feature = feature;
        }

        public IFeatureInfo Feature { get; }
    }
}
