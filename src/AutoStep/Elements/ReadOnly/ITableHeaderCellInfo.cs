namespace AutoStep.Elements.Metadata
{
    /// <summary>
    /// Metadata for a table header cell.
    /// </summary>
    public interface ITableHeaderCellInfo : IPositionalElementInfo
    {
        /// <summary>
        /// Gets the (optional) header name.
        /// </summary>
        string? HeaderName { get; }
    }
}
