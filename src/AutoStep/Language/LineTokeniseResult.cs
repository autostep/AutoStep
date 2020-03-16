using System.Collections.Generic;

namespace AutoStep.Language
{
    /// <summary>
    /// Represents the result of a line tokenisation.
    /// </summary>
    /// <typeparam name="TStateIndicator">The state value used to indicate the end state of the tokeniser.</typeparam>
    public class LineTokeniseResult<TStateIndicator>
        where TStateIndicator : struct
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LineTokeniseResult{TStateIndicator}"/> class for an IEnumerable.
        /// </summary>
        /// <param name="endState">The end state of the tokeniser.</param>
        /// <param name="tokens">The set of tokens.</param>
        public LineTokeniseResult(TStateIndicator endState, IEnumerable<LineToken> tokens)
        {
            EndState = endState;
            Tokens = tokens;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LineTokeniseResult{TStateIndicator}"/> class for a fixed array.
        /// </summary>
        /// <param name="endState">The end state of the tokeniser.</param>
        /// <param name="tokens">The set of tokens.</param>
        public LineTokeniseResult(TStateIndicator endState, params LineToken[] tokens)
        {
            EndState = endState;
            Tokens = tokens;
        }

        /// <summary>
        /// Gets the end state of the tokeniser.
        /// </summary>
        public TStateIndicator EndState { get; }

        /// <summary>
        /// Gets the set of tokens.
        /// </summary>
        public IEnumerable<LineToken> Tokens { get; }
    }
}
