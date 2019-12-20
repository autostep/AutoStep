using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Antlr4.Runtime;
using AutoStep.Compiler.Parser;

namespace AutoStep.Compiler
{
    /// <summary>
    /// Listener attached to the AutoStep parser that generates <see cref="CompilerMessage"/> items from any raised syntax errors.
    /// </summary>
    internal class ParserErrorListener : BaseErrorListener
    {
        private readonly string? sourceName;
        private readonly ITokenStream tokenStream;
        private readonly List<CompilerMessage> messages;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParserErrorListener"/> class.
        /// </summary>
        /// <param name="sourceName">The source name.</param>
        /// <param name="tokenStream">The token stream.</param>
        public ParserErrorListener(string? sourceName, ITokenStream tokenStream)
        {
            this.sourceName = sourceName;
            this.tokenStream = tokenStream;
            this.messages = new List<CompilerMessage>();
        }

        /// <summary>
        /// Gets the set of compiler messages.
        /// </summary>
        public IReadOnlyList<CompilerMessage> ParserErrors => messages;

        /// <inheritdoc/>
        public override void SyntaxError(TextWriter output, IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
        {
            string finalMsg = msg;
            IToken startingSymbol = offendingSymbol;

            switch (offendingSymbol.Type)
            {
                case AutoStepParser.Eof:
                    finalMsg = "Unexpected end of file";
                    break;
                case AutoStepParser.WORD:
                    finalMsg = offendingSymbol.Text switch
                    {
                        "@" => "Bad tag format. Tag must have the format '@tagName'",
                        "$" => "Bad option format. Option must the format '$optionName', " +
                               "optionally with a value separated by ':', e.g. '$optionName:value'",
                        _ => msg
                    };
                    break;
                case AutoStepParser.ARG_ERR_UNEXPECTEDTERMINATOR:
                    finalMsg = "Quoted argument has not been closed.";

                    // Search back for the preceding symbol that opened the quote.
                    startingSymbol = GetPrecedingToken(offendingSymbol, AutoStepParser.OPEN_QUOTE);
                    charPositionInLine = startingSymbol.Column;

                    // Get the preceding token for the offending new line
                    offendingSymbol = GetPrecedingToken(offendingSymbol);

                    break;
            }

            var errorLength = offendingSymbol.StopIndex - startingSymbol.StartIndex;

            if (errorLength < 0)
            {
                errorLength = 0;
            }

            var compileMsg = new CompilerMessage(
                sourceName,
                CompilerMessageLevel.Error,
                CompilerMessageCode.SyntaxError,
                string.Format(CultureInfo.CurrentCulture, CompilerMessages.SyntaxError, finalMsg),
                line,
                charPositionInLine + 1, // Antlr uses 0 offset, but 1 offset for message display feels more natural,
                line,
                charPositionInLine + 1 + errorLength);

            messages.Add(compileMsg);
        }

        /// <summary>
        /// Get the first token of specified type that comes before the specified token in the stream.
        /// </summary>
        /// <param name="currentToken">The token to seek back from.</param>
        /// <param name="tokenType">The token type to look for.</param>
        /// <returns>The found token, or currentToken if we couldn't find anything.</returns>
        private IToken GetPrecedingToken(IToken currentToken, int tokenType)
        {
            IToken? foundToken = null;
            var currentPosition = currentToken.TokenIndex;

            while (currentPosition > 0 && foundToken == null)
            {
                currentPosition--;
                var previousToken = tokenStream.Get(currentPosition);

                if (previousToken.Type == tokenType)
                {
                    foundToken = previousToken;
                }
            }

            return foundToken ?? currentToken;
        }

        private IToken GetPrecedingToken(IToken currentToken)
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
