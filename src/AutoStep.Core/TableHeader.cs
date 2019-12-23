using System;
using System.Collections.Generic;

namespace AutoStep.Core
{
    /// <summary>
    /// Represents the header of a table.
    /// </summary>
    public class TableHeader : BuiltElement
    {
        private List<TableHeaderCell> headers = new List<TableHeaderCell>();

        /// <summary>
        /// Gets the list of headers.
        /// </summary>
        public IReadOnlyList<TableHeaderCell> Headers => headers;

        /// <summary>
        /// Add a header.
        /// </summary>
        /// <param name="header">The header.</param>
        public void AddHeader(TableHeaderCell header)
        {
            headers.Add(header);
        }
    }
}
