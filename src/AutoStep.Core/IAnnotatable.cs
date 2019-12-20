using System.Collections.Generic;

namespace AutoStep.Core
{
    /// <summary>
    /// Indicates that a built element can have annotations applied to it.
    /// </summary>
    public interface IAnnotatable
    {
        /// <summary>
        /// Gets the annotations applied to the feature, in applied order.
        /// </summary>
        List<AnnotationElement> Annotations { get; }
    }
}
