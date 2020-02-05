namespace AutoStep
{
    /// <summary>
    /// Represents the state of the line tokeniser after a line has been tokenised.
    /// </summary>
    public enum LineTokeniserState
    {
        /// <summary>
        /// Default state, either because the tokeniser could not tokenise or because the state is irrelevant.
        /// </summary>
        Default,

        /// <summary>
        /// The last line was the row of a table.
        /// </summary>
        TableRow,

        /// <summary>
        /// The last line was a given-bound statement.
        /// </summary>
        Given,

        /// <summary>
        /// The last line was a when-bound statement.
        /// </summary>
        When,

        /// <summary>
        /// The last line was a then-bound statement.
        /// </summary>
        Then,

        /// <summary>
        /// The last line was an entry block (Feature:, Scenario:, etc).
        /// </summary>
        EntryBlock,
    }
}
