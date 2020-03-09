using System;
using System.Linq;
using Antlr4.Runtime;

namespace AutoStep.Language
{
    /// <summary>
    /// Provides extensions for the token stream to assist in searching it.
    /// </summary>
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
                throw new ArgumentException(TokenStreamExtensionsMessages.AtLeastOneTokenMustBeSpecified, nameof(tokenTypes));
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
        /// Gets the immediately previous token in the token stream (on the default channel), relative to the provided token.
        /// </summary>
        /// <param name="tokenStream">The current token stream to search.</param>
        /// <param name="currentToken">The token to seek back from.</param>
        /// <returns>The found token.</returns>
        public static IToken GetPrecedingToken(this ITokenStream tokenStream, IToken currentToken)
        {
            IToken? foundToken = null;
            var currentPosition = currentToken.TokenIndex;

            while (currentPosition > 0 && foundToken == null)
            {
                currentPosition--;
                var previousToken = tokenStream.Get(currentPosition);

                if (previousToken.Channel == Lexer.DefaultTokenChannel)
                {
                    foundToken = previousToken;
                }
            }

            return foundToken ?? currentToken;
        }
    }
}
