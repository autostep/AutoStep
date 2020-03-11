namespace AutoStep.Elements.Metadata
{
    /// <summary>
    /// Metadata for a tag annotation (@).
    /// </summary>
    public interface ITagInfo : IAnnotationInfo
    {
        /// <summary>
        /// Gets the tag value.
        /// </summary>
        string Tag { get; }
    }
}
