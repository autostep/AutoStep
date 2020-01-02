using System.Collections.Generic;

namespace AutoStep.Core.Elements
{
    /// <summary>
    /// Represents the header of a table.
    /// </summary>
    public class TableHeaderElement : BuiltElement
    {
        private List<TableHeaderCellElement> headers = new List<TableHeaderCellElement>();

        /// <summary>
        /// Gets the list of headers.
        /// </summary>
        public IReadOnlyList<TableHeaderCellElement> Headers => headers;

        /// <summary>
        /// Add a header.
        /// </summary>
        /// <param name="header">The header.</param>
        public void AddHeader(TableHeaderCellElement header)
        {
            headers.Add(header);
        }
    }
}
