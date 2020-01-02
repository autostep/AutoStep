using System.Collections.Generic;

namespace AutoStep.Core.Elements
{
    /// <summary>
    /// A built example with contained table.
    /// </summary>
    public class ExampleElement : BuiltElement, IAnnotatableElement
    {
        /// <summary>
        /// Gets any annotations attached to the example.
        /// </summary>
        public List<AnnotationElement> Annotations { get; } = new List<AnnotationElement>();

        /// <summary>
        /// Gets or sets the table associated to the Example block.
        /// </summary>
        public TableElement Table { get; set; }
    }
}
