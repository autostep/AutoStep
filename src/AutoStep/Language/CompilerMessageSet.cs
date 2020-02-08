using System;
using System.Collections.Generic;
using System.Text;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using AutoStep.Elements;

namespace AutoStep.Language
{
    internal class CompilerMessageSet
    {
        private readonly string? sourceName;
        private readonly ITokenStream tokenStream;
        private readonly List<CompilerMessage> messages = new List<CompilerMessage>();

        public bool AnyErrorMessages { get; private set; } = false;

        /// <summary>
        /// Gets the list of compiler messages generated during the visit process.
        /// </summary>
        public IReadOnlyList<CompilerMessage> Messages => messages;

        public CompilerMessageSet(string? sourceName, ITokenStream tokenStream)
        {
            this.sourceName = sourceName;
            this.tokenStream = tokenStream;
        }

        public void Clear()
        {
            messages.Clear();
            AnyErrorMessages = false;
        }

        /// <summary>
        /// Adds a compiler message.
        /// </summary>
        /// <param name="context">The Antlr parser context that is covered by this message.</param>
        /// <param name="level">Message level.</param>
        /// <param name="code">Message code.</param>
        /// <param name="args">Any arguments used to prepare the message string.</param>
        public void Add(ParserRuleContext context, CompilerMessageLevel level, CompilerMessageCode code, params object[] args)
        {
            Add(CreateMessage(context, level, code, args));
        }

        /// <summary>
        /// Adds a compiler message.
        /// </summary>
        /// <param name="element">A built positional element that is covered by the message.</param>
        /// <param name="level">Message level.</param>
        /// <param name="code">Message code.</param>
        /// <param name="args">Any arguments used to prepare the message string.</param>
        public void Add(PositionalElement element, CompilerMessageLevel level, CompilerMessageCode code, params object[] args)
        {
            Add(CreateMessage(element, level, code, args));
        }

        /// <summary>
        /// Adds a compiler message, where the message should stop at the token immediately preceding the context's stop token.
        /// </summary>
        /// <param name="context">The Antlr parser context that is covered by this message.</param>
        /// <param name="level">Message level.</param>
        /// <param name="code">Message code.</param>
        /// <param name="args">Any arguments used to prepare the message string.</param>
        public void AddStoppingAtPrecedingToken(ParserRuleContext context, CompilerMessageLevel level, CompilerMessageCode code, params object[] args)
        {
            Add(CreateMessage(level, code, context.Start, tokenStream.GetPrecedingToken(context.Stop), args));
        }

        /// <summary>
        /// Adds a compiler message.
        /// </summary>
        /// <param name="context">The Antlr terminal node that is covered by this message.</param>
        /// <param name="level">Message level.</param>
        /// <param name="code">Message code.</param>
        /// <param name="args">Any arguments used to prepare the message string.</param>
        public void Add(ITerminalNode context, CompilerMessageLevel level, CompilerMessageCode code, params object[] args)
        {
            Add(CreateMessage(level, code, context.Symbol, context.Symbol, args));
        }

        /// <summary>
        /// Adds a compiler message.
        /// </summary>
        /// <param name="level">Message level.</param>
        /// <param name="code">Message code.</param>
        /// <param name="start">The Antlr token at which the message starts.</param>
        /// <param name="stop">The Antlr token at which the message stops.</param>
        /// <param name="args">Any arguments used to prepare the message string.</param>
        public void Add(CompilerMessageLevel level, CompilerMessageCode code, IToken start, IToken stop, params object[] args)
        {
            Add(CreateMessage(level, code, start, stop, args));
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
        public void Add(CompilerMessageLevel level, CompilerMessageCode code, int lineStart, int colStart, int lineEnd, int colEnd, params object[] args)
        {
            var message = CreateMessage(level, code, lineStart, colStart, lineEnd, colEnd, args);

            Add(message);
        }

        /// <summary>
        /// Create a compiler message.
        /// </summary>
        /// <param name="context">The Antlr parser context that is covered by this message.</param>
        /// <param name="level">Message level.</param>
        /// <param name="code">Message code.</param>
        /// <param name="args">Any arguments used to prepare the message string.</param>
        /// <returns>The created message.</returns>
        public CompilerMessage CreateMessage(ParserRuleContext context, CompilerMessageLevel level, CompilerMessageCode code, params object[] args)
            => CompilerMessageFactory.Create(sourceName, context, level, code, args);

        /// <summary>
        /// Create a compiler message.
        /// </summary>
        /// <param name="element">A built positional element that is covered by the message.</param>
        /// <param name="level">Message level.</param>
        /// <param name="code">Message code.</param>
        /// <param name="args">Any arguments used to prepare the message string.</param>
        /// <returns>The created message.</returns>
        public CompilerMessage CreateMessage(PositionalElement element, CompilerMessageLevel level, CompilerMessageCode code, params object[] args)
            => CompilerMessageFactory.Create(sourceName, element, level, code, args);

        /// <summary>
        /// Create a compiler message.
        /// </summary>
        /// <param name="level">Message level.</param>
        /// <param name="code">Message code.</param>
        /// <param name="start">The Antlr token at which the message starts.</param>
        /// <param name="stop">The Antlr token at which the message stops.</param>
        /// <param name="args">Any arguments used to prepare the message string.</param>
        /// <returns>The created message.</returns>
        public CompilerMessage CreateMessage(CompilerMessageLevel level, CompilerMessageCode code, IToken start, IToken stop, params object[] args)
            => CompilerMessageFactory.Create(sourceName, level, code, start, stop, args);

        public void AddRange(IEnumerable<CompilerMessage> messages)
        {
            foreach (var newMessage in messages)
            {
                Add(newMessage);
            }
        }

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
        public CompilerMessage CreateMessage(CompilerMessageLevel level, CompilerMessageCode code, int lineStart, int colStart, int lineEnd, int colEnd, params object[] args)
            => CompilerMessageFactory.Create(sourceName, level, code, lineStart, colStart, lineEnd, colEnd, args);

        /// <summary>
        /// Add a compiler message to the set output by the visitor.
        /// </summary>
        /// <param name="message">The message.</param>
        public void Add(CompilerMessage message)
        {
            if (message is null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            if (message.Level == CompilerMessageLevel.Error)
            {
                AnyErrorMessages = true;
            }

            messages.Add(message);
        }
    }
}
