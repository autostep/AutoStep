using System.Collections.Generic;
using AutoStep.Elements.ReadOnly;

namespace AutoStep.Elements
{
    /// <summary>
    /// A built example with contained table.
    /// </summary>
    public class ExampleElement : BuiltElement, IAnnotatableElement, IExampleInfo
    {
        /// <summary>
        /// Gets any annotations attached to the example.
        /// </summary>
        public List<AnnotationElement> Annotations { get; } = new List<AnnotationElement>();

        IReadOnlyList<IAnnotationInfo> IExampleInfo.Annotations => Annotations;

        /// <summary>
        /// Gets or sets the table associated to the Example block.
        /// </summary>
        public TableElement? Table { get; set; }

        ITableInfo IExampleInfo.Table => Table;
    }
}
