using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Antlr4.Runtime;

namespace AutoStep.Compiler
{
    internal static class TokenStreamExtensions
    {
        /// <summary>
        /// Get the first token of one of the specified types that comes before the specified token in the stream.
        /// </summary>
        /// <param name="tokenStream">The current token stream to search.</param>
        /// <param name="currentToken">The token to seek back from.</param>
        /// <param name="tokenTypes">The token type to look for.</param>
        /// <returns>The found token, or currentToken if we couldn't find anything.</returns>
        public static IToken GetPrecedingToken(this ITokenStream tokenStream, IToken currentToken, params int[] tokenTypes)
        {
            if (tokenTypes.Length == 0)
            {
                throw new ArgumentException("At least one token type must be specified", nameof(tokenTypes));
            }

            IToken? foundToken = null;
            var currentPosition = currentToken.TokenIndex;

            while (currentPosition > 0 && foundToken == null)
            {
                currentPosition--;
                var previousToken = tokenStream.Get(currentPosition);

                if (tokenTypes.Contains(previousToken.Type))
                {
                    foundToken = previousToken;
                }
            }

            return foundToken ?? currentToken;
        }

        /// <summary>
        /// Gets the immediately previous token in the token stream, relative to the provided token.
        /// </summary>
        /// <param name="tokenStream">The current token stream to search.</param>
        /// <param name="currentToken">The token to seek back from.</param>
        /// <returns>The found token.</returns>
        public static IToken GetPrecedingToken(this ITokenStream tokenStream, IToken currentToken)
        {
            var newPos = currentToken.TokenIndex - 1;

            if (newPos > 0)
            {
                return tokenStream.Get(newPos);
            }
            else
            {
                return currentToken;
            }
        }
    }
}
