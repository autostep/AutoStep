namespace AutoStep.Core.Elements
{
    /// <summary>
    /// Represents a table cell. A cell's value is treated as a statement argument.
    /// </summary>
    public class TableCellElement : PositionalElement
    {
        /// <summary>
        /// Gets or sets the cell's value.
        /// </summary>
        public StepArgumentElement Value { get; set; }
    }
}
