using System.Collections.Generic;

namespace AutoStep.Core
{

    public class BuiltContent
    {
        public IEnumerable<BuiltStepDefinition> Steps { get; set; }

        public BuiltFeature Feature { get; set; }
    }
}
