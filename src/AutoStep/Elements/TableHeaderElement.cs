using System.Collections.Generic;
using AutoStep.Elements.ReadOnly;

namespace AutoStep.Elements
{
    /// <summary>
    /// Represents the header of a table.
    /// </summary>
    public class TableHeaderElement : BuiltElement, ITableHeaderInfo
    {
        private List<TableHeaderCellElement> headers = new List<TableHeaderCellElement>();

        /// <summary>
        /// Gets the list of headers.
        /// </summary>
        public IReadOnlyList<TableHeaderCellElement> Headers => headers;

        IReadOnlyList<ITableHeaderCellInfo> ITableHeaderInfo.Headers => headers;

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
