using System.Collections.Generic;
using AutoStep.Elements.Metadata;

namespace AutoStep.Elements.Test
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

        /// <summary>
        /// Gets any annotations attached to the example (read-only).
        /// </summary>
        IReadOnlyList<IAnnotationInfo> IExampleInfo.Annotations => Annotations;

        /// <summary>
        /// Gets or sets the table associated to the Example block.
        /// </summary>
        public TableElement? Table { get; set; }

        /// <summary>
        /// Gets the read-only table information.
        /// </summary>
        ITableInfo IExampleInfo.Table => Table ?? throw new LanguageEngineAssertException();
    }
}
