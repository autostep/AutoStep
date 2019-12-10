using System.Collections.Generic;

namespace AutoStep.Core
{
    public interface IAnnotatable
    {
        List<AnnotationElement> Annotations { get; }
    }
}
