using System.Collections.Generic;

namespace AutoStep.Elements.Test
{
    /// <summary>
    /// Indicates that a built element can have annotations applied to it.
    /// </summary>
    public interface IAnnotatableElement
    {
        /// <summary>
        /// Gets the annotations applied to the feature, in applied order.
        /// </summary>
        List<AnnotationElement> Annotations { get; }
    }
}
