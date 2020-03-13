namespace AutoStep.Language
{
    /// <summary>
    /// Defines an interface for a class that can tokenise lines of text and output line tokens.
    /// </summary>
    /// <typeparam name="TStateIndicator">The type of the value used to indicate the tokeniser state.</typeparam>
    internal interface ILineTokeniser<TStateIndicator>
        where TStateIndicator : struct
    {
        /// <summary>
        /// Tokenises a given line of text.
        /// </summary>
        /// <param name="text">The text to tokenise (no line terminators expected).</param>
        /// <param name="lastState">The state of the tokeniser as returned from this method for the previous line in a file.</param>
        /// <returns>The result of tokenisation.</returns>
        LineTokeniseResult<TStateIndicator> Tokenise(string text, TStateIndicator lastState);
    }
}
