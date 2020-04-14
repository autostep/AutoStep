using System;
using System.Text;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using AutoStep.Language.Interaction.Parser;
using AutoStep.Language.Position;

namespace AutoStep.Language.Interaction.Visitors
{
    /// <summary>
    /// Base class for all autostep visitors.
    /// </summary>
    /// <typeparam name="TVisitResult">The visitor result.</typeparam>
    internal abstract class BaseAutoStepInteractionVisitor<TVisitResult> : AutoStepInteractionsParserBaseVisitor<TVisitResult>
        where TVisitResult : class
    {
        private const string DocCommentsMarker = "##";
        private readonly StringBuilder pooledDocBlockBuilder = new StringBuilder();

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseAutoStepInteractionVisitor{TVisitResult}"/> class.
        /// </summary>
        /// <param name="sourceName">The source name.</param>
        /// <param name="tokenStream">The token stream.</param>
        /// <param name="compilerOptions">The compiler options.</param>
        /// <param name="positionIndex">The position index (or null if not in use).</param>
        protected BaseAutoStepInteractionVisitor(string? sourceName, ITokenStream tokenStream, InteractionsCompilerOptions compilerOptions, PositionIndex? positionIndex)
            : this(sourceName, tokenStream, new TokenStreamRewriter(tokenStream), compilerOptions, positionIndex)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseAutoStepInteractionVisitor{TVisitResult}"/> class.
        /// </summary>
        /// <param name="sourceName">The source name.</param>
        /// <param name="tokenStream">The token stream.</param>
        /// <param name="rewriter">The shared rewriter.</param>
        /// <param name="compilerOptions">The compiler options.</param>
        /// <param name="positionIndex">The position index (or null if not in use).</param>
        protected BaseAutoStepInteractionVisitor(string? sourceName, ITokenStream tokenStream, TokenStreamRewriter rewriter, InteractionsCompilerOptions compilerOptions, PositionIndex? positionIndex)
        {
            SourceName = sourceName;
            TokenStream = tokenStream;
            Rewriter = rewriter;
            MessageSet = new CompilerMessageSet(sourceName, tokenStream);
            CompilerOptions = compilerOptions;
            PositionIndex = positionIndex;
        }

        /// <summary>
        /// Gets a value indicating whether the visitor currently considers itself successful.
        /// </summary>
        public bool Success => !MessageSet.AnyErrorMessages;

        /// <summary>
        /// Gets the set of compiler messages.
        /// </summary>
        public CompilerMessageSet MessageSet { get; }

        /// <summary>
        /// Gets or sets the result of the visitor.
        /// </summary>
        public TVisitResult? Result { get; protected set; }

        /// <summary>
        /// Gets the source name (if there is one).
        /// </summary>
        public string? SourceName { get; }

        /// <summary>
        /// Gets the provided compiler options.
        /// </summary>
        public InteractionsCompilerOptions CompilerOptions { get; }

        /// <summary>
        /// Gets the position index.
        /// </summary>
        public PositionIndex? PositionIndex { get; }

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
            MessageSet.Clear();
        }

        /// <summary>
        /// Merge success state and any messages from another visitor and reset that visitor.
        /// </summary>
        /// <typeparam name="TOtherVisitorResult">Type of the other visitor.</typeparam>
        /// <param name="other">The message.</param>
        protected void MergeVisitorAndReset<TOtherVisitorResult>(BaseAutoStepInteractionVisitor<TOtherVisitorResult> other)
            where TOtherVisitorResult : class
        {
            MessageSet.AddRange(other.MessageSet.Messages);

            other.Reset();
        }

        /// <summary>
        /// Retrieves the text for a terminal node containing a quoted string.
        /// </summary>
        /// <param name="node">The antlr node.</param>
        /// <returns>The text block.</returns>
        protected string GetTextFromStringToken(ITerminalNode node)
        {
            // The text for a string node is just minus 1 character on either side.
            var symbol = node.Symbol;

            var interval = new Interval(symbol.StartIndex + 1, symbol.StopIndex - 1);

            if (interval.Length > 0)
            {
                return symbol.InputStream.GetText(interval);
            }

            return string.Empty;
        }

        /// <summary>
        /// Retrieve the documentation block for an element (or null if there isn't one).
        /// </summary>
        /// <param name="owningContext">The parser block.</param>
        /// <returns>The documentation content.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Globalization",
            "CA1303:Do not pass literals as localized parameters",
            Justification = "Comment marker is fixed in the language specification.")]
        protected string? GetDocumentationBlockForElement(ParserRuleContext owningContext)
        {
            string? documentation = null;
            ReadOnlySpan<char> markerSequence = DocCommentsMarker.AsSpan();

            // Get preceding doc contents.
            if (TokenStream is CommonTokenStream commonStream)
            {
                // Get doc-comment blocks before the element.
                var hiddenTokens = commonStream.GetHiddenTokensToLeft(owningContext.Start.TokenIndex);

                // First whitespace.
                int knownSpacing = 0;
                bool firstLineWritten = false;
                bool determinedKnownSpacing = false;

                foreach (var token in hiddenTokens)
                {
                    if (token.Type == AutoStepInteractionsLexer.TEXT_DOC_COMMENT)
                    {
                        if (firstLineWritten)
                        {
                            pooledDocBlockBuilder.AppendLine();
                        }
                        else
                        {
                            firstLineWritten = true;
                        }

                        var text = token.Text.AsSpan();

                        // Find the position of the marker characters.
                        var markerPos = text.IndexOf(markerSequence, StringComparison.InvariantCulture);

                        // Move beyond the marker characters.
                        text = text.Slice(markerPos + 2);

                        // If the spacing is not known, then work it out.
                        if (text.Length > 0)
                        {
                            var currentPos = 0;

                            var endPoint = determinedKnownSpacing ? knownSpacing : text.Length;

                            // Get the whitespace characters.
                            while (currentPos < endPoint)
                            {
                                if (!char.IsWhiteSpace(text[currentPos]))
                                {
                                    if (!determinedKnownSpacing)
                                    {
                                        knownSpacing = currentPos;
                                        determinedKnownSpacing = true;
                                    }

                                    break;
                                }

                                currentPos++;
                            }

                            text = text.Slice(currentPos);
                        }

                        // Take off the whitespace.
                        pooledDocBlockBuilder.Append(text);
                    }
                }

                documentation = pooledDocBlockBuilder.ToString();

                if (string.IsNullOrWhiteSpace(documentation))
                {
                    documentation = null;
                }

                pooledDocBlockBuilder.Clear();
            }

            return documentation;
        }
    }
}
