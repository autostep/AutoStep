using System;
using System.Collections.Generic;
using AutoStep.Elements.StepTokens;

namespace AutoStep.Elements
{
    /// <summary>
    /// Represents a table cell. A cell's value is treated as a statement argument.
    /// </summary>
    public class TableCellElement : PositionalElement
    {
        private List<StepToken> tokens = new List<StepToken>();

        /// <summary>
        /// Gets or sets the text of the cell.
        /// </summary>
        public string? Text { get; set; }

        /// <summary>
        /// Gets the set of tokens.
        /// </summary>
        internal IReadOnlyList<StepToken> Tokens => tokens;

        /// <summary>
        /// Adds a token to the cell.
        /// </summary>
        /// <param name="token">The token to add.</param>
        internal void AddToken(StepToken token)
        {
            if (token is null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            tokens.Add(token);
        }
    }
}
