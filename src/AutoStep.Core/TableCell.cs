using System;
using System.Collections.Generic;

namespace AutoStep.Core
{
    /// <summary>
    /// Represents a table cell. A cell's value is treated as a statement argument.
    /// </summary>
    public class TableCell : PositionalElement
    {
        /// <summary>
        /// Gets or sets the cell's value.
        /// </summary>
        public StepArgument Value { get; set; }
    }
}
