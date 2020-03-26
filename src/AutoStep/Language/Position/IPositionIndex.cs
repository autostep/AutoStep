namespace AutoStep.Language.Position
{
    /// <summary>
    /// Provides access to details about the syntax tree at a given position in a file.
    /// </summary>
    public interface IPositionIndex
    {
        /// <summary>
        /// Retrieve a block of position info for a given line and column in the file.
        ///
        /// The returned information contains the active 'scopes' (i.e. Scenario, Feature, etc) at the given point
        /// (even if there are no tokens at that position).
        ///
        /// If the cursor position lands on a line that contains known tokens, the result will include those tokens.
        /// The result will identify the specific token (or the closest preceding one) if the cursor is on/near a token.
        /// </summary>
        /// <param name="line">The line number (starting from 1).</param>
        /// <param name="column">The column (starting from 1).</param>
        /// <returns>A position info block.</returns>
        PositionInfo Lookup(int line, int column);
    }
}
