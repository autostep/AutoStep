namespace AutoStep.Elements.Metadata
{
    /// <summary>
    /// Metadata for an element that may end before the line terminates.
    /// </summary>
    public interface IPositionalElementInfo : IElementInfo
    {
        /// <summary>
        /// Gets the end column position (1-based).
        /// </summary>
        int EndColumn { get; }

        /// <summary>
        /// Gets the end line.
        /// </summary>
        int EndLine { get; }
    }
}
