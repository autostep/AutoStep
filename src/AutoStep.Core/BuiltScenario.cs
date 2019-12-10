using System.Collections.Generic;

namespace AutoStep.Core
{
    public class BuiltScenario : BuiltStepCollection, IAnnotatable
    {
        public List<AnnotationElement> Annotations { get; } = new List<AnnotationElement>();

        public string Name { get; set; }

        public string Description { get; set; }

        public List<StepReference> Steps { get; } = new List<StepReference>();
    }
}
