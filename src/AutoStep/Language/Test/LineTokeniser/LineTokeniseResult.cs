using System.Collections.Generic;

namespace AutoStep
{
    /// <summary>
    /// Represents the result of a line tokenisation.
    /// </summary>
    public class LineTokeniseResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LineTokeniseResult"/> class for an IEnumerable.
        /// </summary>
        /// <param name="endState">The end state of the tokeniser.</param>
        /// <param name="tokens">The set of tokens.</param>
        public LineTokeniseResult(LineTokeniserState endState, IEnumerable<LineToken> tokens)
        {
            EndState = endState;
            Tokens = tokens;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LineTokeniseResult"/> class for a fixed array.
        /// </summary>
        /// <param name="endState">The end state of the tokeniser.</param>
        /// <param name="tokens">The set of tokens.</param>
        public LineTokeniseResult(LineTokeniserState endState, params LineToken[] tokens)
        {
            EndState = endState;
            Tokens = tokens;
        }

        /// <summary>
        /// Gets the end state of the tokeniser.
        /// </summary>
        public LineTokeniserState EndState { get; }

        /// <summary>
        /// Gets the set of tokens.
        /// </summary>
        public IEnumerable<LineToken> Tokens { get; }
    }
}
