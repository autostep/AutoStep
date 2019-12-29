using System.Collections.Generic;

namespace AutoStep.Core
{
    /// <summary>
    /// A built example with contained table.
    /// </summary>
    public class BuiltExample : BuiltElement, IAnnotatable
    {
        /// <summary>
        /// Gets any annotations attached to the example.
        /// </summary>
        public List<AnnotationElement> Annotations { get; } = new List<AnnotationElement>();

        /// <summary>
        /// Gets or sets the table associated to the Example block.
        /// </summary>
        public BuiltTable Table { get; set; }
    }
}
