using AutoStep.Elements.ReadOnly;

namespace AutoStep.Elements
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
