using System.Collections.Generic;
using AutoStep.Elements.Metadata;

namespace AutoStep.Elements.Test
{
    /// <summary>
    /// Represents the header of a table.
    /// </summary>
    public class TableHeaderElement : BuiltElement, ITableHeaderInfo
    {
        private readonly List<TableHeaderCellElement> headers = new List<TableHeaderCellElement>();

        /// <summary>
        /// Gets the list of headers.
        /// </summary>
        public IReadOnlyList<TableHeaderCellElement> Headers => headers;

        /// <inheritdoc/>
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
