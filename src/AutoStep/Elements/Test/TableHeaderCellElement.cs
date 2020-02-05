using AutoStep.Elements.Metadata;

namespace AutoStep.Elements.Test
{
    /// <summary>
    /// Represents a table header cell.
    /// </summary>
    public class TableHeaderCellElement : PositionalElement, ITableHeaderCellInfo
    {
        /// <summary>
        /// Gets or sets the name of the header.
        /// </summary>
        public string? HeaderName { get; set; }
    }
}
