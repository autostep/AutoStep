using System;
using System.Collections.Generic;
using System.Linq;
using AutoStep.Elements;

namespace AutoStep.Language.Position
{
    /// <summary>
    /// Represents the entry for a line in the position index.
    /// </summary>
    internal class LineEntry : PositionEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LineEntry"/> class.
        /// </summary>
        /// <param name="scopes">The set of scopes that apply to the line.</param>
        public LineEntry(IReadOnlyList<BuiltElement> scopes)
            : base(scopes)
        {
        }

        /// <summary>
        /// Gets the set of tokens on the line (null if no tokens present).
        /// </summary>
        public List<PositionLineToken>? Tokens { get; private set; }

        /// <summary>
        /// Add a token to a line. Tokens must be added in column order.
        /// </summary>
        /// <param name="token">The token.</param>
        public void AddToken(PositionLineToken token)
        {
            if (token is null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            if (Tokens is null)
            {
                Tokens = new List<PositionLineToken>();
            }

            // Try and add to the end by default.
            if (Tokens.Count == 0 || token.StartColumn > Tokens.Last().EndColumn)
            {
                Tokens.Add(token);
            }
            else
            {
                throw new InvalidOperationException(PositionMessages.TokensMustBeAddedInColumnOrder);
            }
        }
    }
}
