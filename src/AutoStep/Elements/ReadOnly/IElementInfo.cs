namespace AutoStep.Elements.Metadata
{
    /// <summary>
    /// Metadata for a code element.
    /// </summary>
    public interface IElementInfo
    {
        /// <summary>
        /// Gets the 1-based line number on which the element appears.
        /// </summary>
        int SourceLine { get; }

        /// <summary>
        /// Gets the 1-based column number at which the element starts.
        /// </summary>
        int StartColumn { get; }
    }
}
