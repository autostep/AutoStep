using System;
using System.Collections.Generic;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using AutoStep.Compiler.Parser;
using AutoStep.Elements;

namespace AutoStep.Compiler
{
    /// <summary>
    /// Base class for all autostep visitors.
    /// </summary>
    /// <typeparam name="TVisitResult">The visitor result.</typeparam>
    internal abstract class BaseAutoStepVisitor<TVisitResult> : AutoStepParserBaseVisitor<TVisitResult>
        where TVisitResult : class
    {
        private readonly List<CompilerMessage> messages = new List<CompilerMessage>();

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseAutoStepVisitor{TVisitResult}"/> class.
        /// </summary>
        /// <param name="sourceName">The source name.</param>
        /// <param name="tokenStream">The token stream.</param>
        protected BaseAutoStepVisitor(string? sourceName, ITokenStream tokenStream)
            : this(sourceName, tokenStream, new TokenStreamRewriter(tokenStream))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseAutoStepVisitor{TVisitResult}"/> class.
        /// </summary>
        /// <param name="sourceName">The source name.</param>
        /// <param name="tokenStream">The token stream.</param>
        /// <param name="rewriter">The shared rewriter.</param>
        protected BaseAutoStepVisitor(string? sourceName, ITokenStream tokenStream, TokenStreamRewriter rewriter)
        {
            this.SourceName = sourceName;
            this.TokenStream = tokenStream;
            this.Rewriter = rewriter;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the visitor currently considers itself successful.
        /// </summary>
        public bool Success { get; protected set; } = true;

        /// <summary>
        /// Gets the list of compiler messages generated during the visit process.
        /// </summary>
        public IReadOnlyList<CompilerMessage> Messages => messages;

        /// <summary>
        /// Gets or sets the result of the visitor.
        /// </summary>
        public TVisitResult? Result { get; protected set; }

        /// <summary>
        /// Gets the source name (if there is one).
        /// </summary>
        protected string? SourceName { get; }

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
            Success = true;
            Result = null;
            messages.Clear();
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
        /// Adds a compiler message.
        /// </summary>
        /// <param name="context">The Antlr parser context that is covered by this message.</param>
        /// <param name="level">Message level.</param>
        /// <param name="code">Message code.</param>
        /// <param name="args">Any arguments used to prepare the message string.</param>
        protected void AddMessage(ParserRuleContext context, CompilerMessageLevel level, CompilerMessageCode code, params object[] args)
        {
            AddMessage(CreateMessage(context, level, code, args));
        }

        /// <summary>
        /// Adds a compiler message.
        /// </summary>
        /// <param name="element">A built positional element that is covered by the message.</param>
        /// <param name="level">Message level.</param>
        /// <param name="code">Message code.</param>
        /// <param name="args">Any arguments used to prepare the message string.</param>
        protected void AddMessage(PositionalElement element, CompilerMessageLevel level, CompilerMessageCode code, params object[] args)
        {
            AddMessage(CreateMessage(element, level, code, args));
        }

        /// <summary>
        /// Adds a compiler message, where the message should stop at the token immediately preceding the context's stop token.
        /// </summary>
        /// <param name="context">The Antlr parser context that is covered by this message.</param>
        /// <param name="level">Message level.</param>
        /// <param name="code">Message code.</param>
        /// <param name="args">Any arguments used to prepare the message string.</param>
        protected void AddMessageStoppingAtPrecedingToken(ParserRuleContext context, CompilerMessageLevel level, CompilerMessageCode code, params object[] args)
        {
            AddMessage(CreateMessage(level, code, context.Start, TokenStream.GetPrecedingToken(context.Stop), args));
        }

        /// <summary>
        /// Adds a compiler message.
        /// </summary>
        /// <param name="context">The Antlr terminal node that is covered by this message.</param>
        /// <param name="level">Message level.</param>
        /// <param name="code">Message code.</param>
        /// <param name="args">Any arguments used to prepare the message string.</param>
        protected void AddMessage(ITerminalNode context, CompilerMessageLevel level, CompilerMessageCode code, params object[] args)
        {
            AddMessage(CreateMessage(level, code, context.Symbol, context.Symbol, args));
        }

        /// <summary>
        /// Adds a compiler message.
        /// </summary>
        /// <param name="level">Message level.</param>
        /// <param name="code">Message code.</param>
        /// <param name="start">The Antlr token at which the message starts.</param>
        /// <param name="stop">The Antlr token at which the message stops.</param>
        /// <param name="args">Any arguments used to prepare the message string.</param>
        protected void AddMessage(CompilerMessageLevel level, CompilerMessageCode code, IToken start, IToken stop, params object[] args)
        {
            AddMessage(CreateMessage(level, code, start, stop, args));
        }

        /// <summary>
        /// Adds a compiler message.
        /// </summary>
        /// <param name="level">Message level.</param>
        /// <param name="code">Message code.</param>
        /// <param name="lineStart">The line on which the message starts.</param>
        /// <param name="colStart">The column position at which the message starts.</param>
        /// <param name="lineEnd">The line on which the message ends.</param>
        /// <param name="colEnd">The column position at which the message ends.</param>
        /// <param name="args">Any arguments used to prepare the message string.</param>
        protected void AddMessage(CompilerMessageLevel level, CompilerMessageCode code, int lineStart, int colStart, int lineEnd, int colEnd, params object[] args)
        {
            var message = CreateMessage(level, code, lineStart, colStart, lineEnd, colEnd, args);

            AddMessage(message);
        }

        /// <summary>
        /// Create a compiler message.
        /// </summary>
        /// <param name="context">The Antlr parser context that is covered by this message.</param>
        /// <param name="level">Message level.</param>
        /// <param name="code">Message code.</param>
        /// <param name="args">Any arguments used to prepare the message string.</param>
        /// <returns>The created message.</returns>
        protected CompilerMessage CreateMessage(ParserRuleContext context, CompilerMessageLevel level, CompilerMessageCode code, params object[] args)
            => CompilerMessageFactory.Create(SourceName, context, level, code, args);

        /// <summary>
        /// Create a compiler message.
        /// </summary>
        /// <param name="element">A built positional element that is covered by the message.</param>
        /// <param name="level">Message level.</param>
        /// <param name="code">Message code.</param>
        /// <param name="args">Any arguments used to prepare the message string.</param>
        /// <returns>The created message.</returns>
        protected CompilerMessage CreateMessage(PositionalElement element, CompilerMessageLevel level, CompilerMessageCode code, params object[] args)
            => CompilerMessageFactory.Create(SourceName, element, level, code, args);

        /// <summary>
        /// Create a compiler message.
        /// </summary>
        /// <param name="level">Message level.</param>
        /// <param name="code">Message code.</param>
        /// <param name="start">The Antlr token at which the message starts.</param>
        /// <param name="stop">The Antlr token at which the message stops.</param>
        /// <param name="args">Any arguments used to prepare the message string.</param>
        /// <returns>The created message.</returns>
        protected CompilerMessage CreateMessage(CompilerMessageLevel level, CompilerMessageCode code, IToken start, IToken stop, params object[] args)
            => CompilerMessageFactory.Create(SourceName, level, code, start, stop, args);

        /// <summary>
        /// Create a compiler message.
        /// </summary>
        /// <param name="level">Message level.</param>
        /// <param name="code">Message code.</param>
        /// <param name="lineStart">The line on which the message starts.</param>
        /// <param name="colStart">The column position at which the message starts.</param>
        /// <param name="lineEnd">The line on which the message ends.</param>
        /// <param name="colEnd">The column position at which the message ends.</param>
        /// <param name="args">Any arguments used to prepare the message string.</param>
        /// <returns>The created message.</returns>
        protected CompilerMessage CreateMessage(CompilerMessageLevel level, CompilerMessageCode code, int lineStart, int colStart, int lineEnd, int colEnd, params object[] args)
            => CompilerMessageFactory.Create(SourceName, level, code, lineStart, colStart, lineEnd, colEnd, args);

        /// <summary>
        /// Add a compiler message to the set output by the visitor.
        /// </summary>
        /// <param name="message">The message.</param>
        protected void AddMessage(CompilerMessage message)
        {
            if (message is null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            if (message.Level == CompilerMessageLevel.Error)
            {
                Success = false;
            }

            messages.Add(message);
        }

        /// <summary>
        /// Add positional line information to the specified element, using the given parser context for positions.
        /// </summary>
        /// <typeparam name="TElement">The element type.</typeparam>
        /// <param name="element">The element to update.</param>
        /// <param name="ctxt">The Antlr parser context.</param>
        /// <returns>The same input element, after update.</returns>
        protected TElement PositionalLineInfo<TElement>(TElement element, ParserRuleContext ctxt)
            where TElement : PositionalElement
        {
            return PositionalLineInfo(element, ctxt.Start, ctxt.Stop);
        }

        /// <summary>
        /// Add positional line information to the specified element, using the given terminal node for positions.
        /// </summary>
        /// <typeparam name="TElement">The element type.</typeparam>
        /// <param name="element">The element to update.</param>
        /// <param name="ctxt">The Antlr terminal node.</param>
        /// <returns>The same input element, after update.</returns>
        protected TElement PositionalLineInfo<TElement>(TElement element, ITerminalNode ctxt)
            where TElement : PositionalElement
        {
            return PositionalLineInfo(element, ctxt.Symbol, ctxt.Symbol);
        }

        /// <summary>
        /// Add positional line information to the specified element, using the given tokens for start and stop positions.
        /// </summary>
        /// <typeparam name="TElement">The element type.</typeparam>
        /// <param name="element">The element to update.</param>
        /// <param name="start">The start position.</param>
        /// <param name="stop">The stop position.</param>
        /// <returns>The same input element, after update.</returns>
        protected TElement PositionalLineInfo<TElement>(TElement element, IToken start, IToken stop)
            where TElement : PositionalElement
        {
            element.SourceLine = start.Line;
            element.SourceColumn = start.Column + 1;
            element.EndColumn = stop.Column + (stop.StopIndex - stop.StartIndex) + 1;

            return element;
        }

        /// <summary>
        /// Add line information to the specified element, using the given parser context for positions.
        /// </summary>
        /// <typeparam name="TElement">The element type.</typeparam>
        /// <param name="element">The element to update.</param>
        /// <param name="ctxt">The Antlr parser context.</param>
        /// <returns>The same input element, after update.</returns>
        protected TElement LineInfo<TElement>(TElement element, ParserRuleContext ctxt)
            where TElement : BuiltElement
        {
            element.SourceLine = ctxt.Start.Line;
            element.SourceColumn = ctxt.Start.Column + 1;

            return element;
        }

        /// <summary>
        /// Add line information to the specified element, using the given terminal node for positions.
        /// </summary>
        /// <typeparam name="TElement">The element type.</typeparam>
        /// <param name="element">The element to update.</param>
        /// <param name="ctxt">The Antlr terminal node.</param>
        /// <returns>The same input element, after update.</returns>
        protected TElement LineInfo<TElement>(TElement element, ITerminalNode ctxt)
            where TElement : BuiltElement
        {
            element.SourceLine = ctxt.Symbol.Line;
            element.SourceColumn = ctxt.Symbol.Column + 1;

            return element;
        }

        /// <summary>
        /// Merge success state and any messages from another visitor and reset that visitor.
        /// </summary>
        /// <typeparam name="TOtherVisitorResult">Type of the other visitor.</typeparam>
        /// <param name="other">The message.</param>
        protected void MergeVisitorAndReset<TOtherVisitorResult>(BaseAutoStepVisitor<TOtherVisitorResult> other)
            where TOtherVisitorResult : class
        {
            messages.AddRange(other.messages);
            if (!other.Success)
            {
                Success = false;
            }

            other.Reset();
        }
    }
}
