using AutoStep.Elements.ReadOnly;

namespace AutoStep.Elements
{
    /// <summary>
    /// Represents a tag annotation.
    /// </summary>
    public class TagElement : AnnotationElement, ITagInfo
    {
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
