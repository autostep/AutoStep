using AutoStep.Elements.Metadata;

namespace AutoStep.Elements
{
    /// <summary>
    /// Represents a tag annotation.
    /// </summary>
    public class TagElement : AnnotationElement, ITagInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TagElement"/> class.
        /// </summary>
        /// <param name="tag">The tag value.</param>
        public TagElement(string tag)
        {
            Tag = tag;
        }

        /// <summary>
        /// Gets or sets the value of the tag.
        /// </summary>
        public string Tag { get; set; }
    }
}
