using AutoStep.Elements;

namespace AutoStep.Execution
{

    public class FeatureContext : ExecutionContext
    {
        internal FeatureContext(FeatureElement feature)
        {
            Feature = feature;
        }

        public FeatureElement Feature { get; }
    }
}
