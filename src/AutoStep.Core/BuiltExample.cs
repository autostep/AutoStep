using System.Collections.Generic;

namespace AutoStep.Core
{

    public class BuiltExample : BuiltElement, IAnnotatable
    {
        public List<AnnotationElement> Annotations { get; } = new List<AnnotationElement>();

        public BuiltTable Table { get; set; }
    }
}
