using System;
using System.Collections.Generic;

namespace AutoStep.Core
{
    /// <summary>
    /// Represents a table header cell.
    /// </summary>
    public class TableHeaderCell : PositionalElement
    {
        /// <summary>
        /// Gets or sets the name of the header.
        /// </summary>
        public string HeaderName { get; set; }
    }
}
