using System;
using System.Collections.Generic;
using System.Text;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using AutoStep.Elements;
using AutoStep.Language.Interaction.Parser;
using AutoStep.Language.Test.Parser;
using AutoStep.Language.Test.Visitors;

namespace AutoStep.Language.Interaction.Visitors
{
    /// <summary>
    /// Base class for all autostep visitors.
    /// </summary>
    /// <typeparam name="TVisitResult">The visitor result.</typeparam>
    internal abstract class BaseAutoStepInteractionVisitor<TVisitResult> : AutoStepInteractionsParserBaseVisitor<TVisitResult>
        where TVisitResult : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseAutoStepInteractionVisitor{TVisitResult}"/> class.
        /// </summary>
        /// <param name="sourceName">The source name.</param>
        /// <param name="tokenStream">The token stream.</param>
        protected BaseAutoStepInteractionVisitor(string? sourceName, ITokenStream tokenStream)
            : this(sourceName, tokenStream, new TokenStreamRewriter(tokenStream))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseAutoStepInteractionVisitor{TVisitResult}"/> class.
        /// </summary>
        /// <param name="sourceName">The source name.</param>
        /// <param name="tokenStream">The token stream.</param>
        /// <param name="rewriter">The shared rewriter.</param>
        protected BaseAutoStepInteractionVisitor(string? sourceName, ITokenStream tokenStream, TokenStreamRewriter rewriter)
        {
            SourceName = sourceName;
            TokenStream = tokenStream;
            Rewriter = rewriter;
            MessageSet = new CompilerMessageSet(sourceName, tokenStream);
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

    }
}
