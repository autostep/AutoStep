using System.Text;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using AutoStep.Language.Test.Parser;

namespace AutoStep.Language.Test.Visitors
{
    /// <summary>
    /// Base class for all autostep visitors.
    /// </summary>
    /// <typeparam name="TVisitResult">The visitor result.</typeparam>
    internal abstract class BaseTestVisitor<TVisitResult> : AutoStepParserBaseVisitor<TVisitResult>
        where TVisitResult : class
    {
        private readonly CompilerMessageSet messageSet;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseTestVisitor{TVisitResult}"/> class.
        /// </summary>
        /// <param name="sourceName">The source name.</param>
        /// <param name="tokenStream">The token stream.</param>
        protected BaseTestVisitor(string? sourceName, ITokenStream tokenStream)
            : this(sourceName, tokenStream, new TokenStreamRewriter(tokenStream))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseTestVisitor{TVisitResult}"/> class.
        /// </summary>
        /// <param name="sourceName">The source name.</param>
        /// <param name="tokenStream">The token stream.</param>
        /// <param name="rewriter">The shared rewriter.</param>
        protected BaseTestVisitor(string? sourceName, ITokenStream tokenStream, TokenStreamRewriter rewriter)
        {
            SourceName = sourceName;
            TokenStream = tokenStream;
            Rewriter = rewriter;
            messageSet = new CompilerMessageSet(sourceName, tokenStream);
        }

        /// <summary>
        /// Gets a value indicating whether the visitor currently considers itself successful.
        /// </summary>
        public bool Success => !messageSet.AnyErrorMessages;

        /// <summary>
        /// Gets the set of compiler messages.
        /// </summary>
        public CompilerMessageSet MessageSet => messageSet;

        /// <summary>
        /// Gets or sets the result of the visitor.
        /// </summary>
        public TVisitResult? Result { get; protected set; }

        /// <summary>
        /// Gets the source name (if there is one).
        /// </summary>
        public string? SourceName { get; }

        /// <summary>
        /// Gets the token stream.
        /// </summary>
        protected ITokenStream TokenStream { get; }

        /// <summary>
        /// Gets the rewriter.
        /// </summary>
        protected TokenStreamRewriter Rewriter { get; }

        /// <summary>
        /// Reset the visitor ready for the next visit execution.
        /// </summary>
        public virtual void Reset()
        {
            Result = null;
            messageSet.Clear();
        }

        /// <summary>
        /// Merge success state and any messages from another visitor and reset that visitor.
        /// </summary>
        /// <typeparam name="TOtherVisitorResult">Type of the other visitor.</typeparam>
        /// <param name="other">The message.</param>
        protected void MergeVisitorAndReset<TOtherVisitorResult>(BaseTestVisitor<TOtherVisitorResult> other)
            where TOtherVisitorResult : class
        {
            messageSet.AddRange(other.MessageSet.Messages);

            other.Reset();
        }

        /// <summary>
        /// Escape any text between start and stop positions, updating the shared rewriter and returning the escaped text.
        /// </summary>
        /// <param name="start">The start position of the window.</param>
        /// <param name="stop">The stop position of the window.</param>
        /// <param name="replacements">The set of replacements, as 'token id' -> replacement pairs.</param>
        /// <returns>The escaped text.</returns>
        protected string EscapeText(IToken start, IToken stop, params (int Token, string Replacement)[] replacements)
        {
            for (var idx = start.TokenIndex; idx <= stop.TokenIndex; idx++)
            {
                foreach (var rep in replacements)
                {
                    var token = TokenStream.Get(idx);

                    if (token.Type == rep.Token)
                    {
                        // Replace
                        Rewriter.Replace(token, rep.Replacement);
                    }
                }
            }

            return Rewriter.GetText(new Interval(start.TokenIndex, stop.TokenIndex));
        }

        /// <summary>
        /// Escape any text within the given parser rule context, updating the shared rewriter and returning the escaped text.
        /// </summary>
        /// <param name="context">The parser rule context to escape.</param>
        /// <param name="replacements">The set of replacements, as 'token id' -> replacement pairs.</param>
        /// <returns>The escaped text.</returns>
        protected string EscapeText(ParserRuleContext context, params (int Token, string Replacement)[] replacements)
        {
            return EscapeText(context.Start, context.Stop, replacements);
        }

        /// <summary>
        /// Generates the description text from a parsed description context.
        /// Handles indentation of the overall description, and indentation inside it.
        /// </summary>
        /// <param name="descriptionContext">The context.</param>
        /// <returns>The complete description string.</returns>
        protected string? ExtractDescription(AutoStepParser.DescriptionContext descriptionContext)
        {
            if (descriptionContext is null)
            {
                return null;
            }

            var lines = descriptionContext.line();

            if (lines.Length == 0)
            {
                return null;
            }

            var descriptionBuilder = new StringBuilder();

            int? whitespaceRemovalCount = null;
            int? firstTextIndex = null;
            var lastTextIndex = 0;

            // First pass to get our whitespace size and last text position.
            for (var lineIdx = 0; lineIdx < lines.Length; lineIdx++)
            {
                var line = lines[lineIdx];
                var text = line.text();

                if (text is object)
                {
                    if (firstTextIndex == null)
                    {
                        firstTextIndex = lineIdx;
                    }

                    lastTextIndex = lineIdx;
                    var whiteSpaceSymbol = line.WS()?.Symbol;
                    var whiteSpaceSize = 0;

                    if (whiteSpaceSymbol is object)
                    {
                        // This is the size of the whitespace.
                        whiteSpaceSize = 1 + whiteSpaceSymbol.StopIndex - whiteSpaceSymbol.StartIndex;
                    }

                    if (whitespaceRemovalCount is null)
                    {
                        // This is the first item of non-whitespace text we have reached.
                        // Base our initial minimum whitespace on this.
                        whitespaceRemovalCount = whiteSpaceSize;
                    }
                    else if (whiteSpaceSize < whitespaceRemovalCount)
                    {
                        // Bring the whitespace in if the amount of whitespace has changed.
                        // We'll ignore whitespace lengths for lines with no text.
                        whitespaceRemovalCount = whiteSpaceSize;
                    }
                }
            }

            // No point rendering anything if there were no text lines.
            if (firstTextIndex is object)
            {
                // Second pass to render our description, only go up to the last text position.
                for (var lineIdx = firstTextIndex.Value; lineIdx <= lastTextIndex; lineIdx++)
                {
                    var line = lines[lineIdx];
                    var text = line.text();

                    if (text is null)
                    {
                        descriptionBuilder.AppendLine();
                    }
                    else
                    {
                        var wsText = line.WS()?.GetText();
                        if (whitespaceRemovalCount is object && wsText is object)
                        {
                            wsText = wsText.Substring(whitespaceRemovalCount.Value);
                        }

                        // Append all whitespace after the removal amount, plus the text.
                        descriptionBuilder.Append(wsText);
                        descriptionBuilder.Append(text.GetText());

                        if (lineIdx < lastTextIndex)
                        {
                            // Only add the line if we're not at the end.
                            descriptionBuilder.AppendLine();
                        }
                    }
                }
            }
            else
            {
                return null;
            }

            return descriptionBuilder.ToString();
        }
    }
}
