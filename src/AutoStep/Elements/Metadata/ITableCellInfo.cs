using System;
using System.Collections.Generic;
using AutoStep.Elements.StepTokens;

namespace AutoStep.Elements.Metadata
{
    /// <summary>
    /// Metadata for a table cell.
    /// </summary>
    public interface ITableCellInfo : IPositionalElementInfo
    {
        /// <summary>
        /// Gets the text in the cell.
        /// </summary>
        string? Text { get; }

        /// <summary>
        /// Gets the set of all tokens in the cell.
        /// </summary>
        internal IReadOnlyList<StepToken> Tokens { get; }
    }
}
