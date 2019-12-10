using System.Collections.Generic;

namespace AutoStep.Core
{

    public class BuiltFeature : BuiltElement, IAnnotatable
    {
        public List<AnnotationElement> Annotations { get; } = new List<AnnotationElement>();

        public string Name { get; set; }

        public string Description { get; set; }

        public List<StepReference> Background { get; } = new List<StepReference>();

        public List<BuiltScenario> Scenarios { get; } = new List<BuiltScenario>();
    }
}
