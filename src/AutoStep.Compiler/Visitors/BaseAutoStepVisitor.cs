using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using AutoStep.Compiler.Parser;
using AutoStep.Core;
using AutoStep.Core.Elements;

namespace AutoStep.Compiler
{
    internal abstract class BaseAutoStepVisitor<TVisitResult> : AutoStepParserBaseVisitor<TVisitResult>
        where TVisitResult : class
    {
        protected readonly string? sourceName;
        protected readonly ITokenStream tokenStream;
        protected readonly TokenStreamRewriter rewriter;

        private readonly List<CompilerMessage> messages = new List<CompilerMessage>();

        protected BaseAutoStepVisitor(string? sourceName, ITokenStream tokenStream)
            : this(sourceName, tokenStream, new TokenStreamRewriter(tokenStream))
        {
        }

        protected BaseAutoStepVisitor(string? sourceName, ITokenStream tokenStream, TokenStreamRewriter rewriter)
        {
            this.sourceName = sourceName;
            this.tokenStream = tokenStream;
            this.rewriter = rewriter;
        }

        public bool Success { get; protected set; } = true;

        /// <summary>
        /// Gets the list of compiler messages generated during the compilation process.
        /// </summary>
        public IReadOnlyList<CompilerMessage> Messages => messages;

        public TVisitResult? Result { get; protected set; }

        public virtual void Reset()
        {
            Success = true;
            Result = null;
            messages.Clear();
        }

        protected string EscapeText(IToken start, IToken stop, params (int Token, string Replacement)[] replacements)
        {
            for (var idx = start.TokenIndex; idx <= stop.TokenIndex; idx++)
            {
                foreach (var rep in replacements)
                {
                    var token = tokenStream.Get(idx);

                    if (token.Type == rep.Token)
                    {
                        // Replace
                        rewriter.Replace(token, rep.Replacement);
                    }
                }
            }

            return rewriter.GetText(new Interval(start.TokenIndex, stop.TokenIndex));
        }

        protected string EscapeText(ParserRuleContext context, params (int Token, string Replacement)[] replacements)
        {
            return EscapeText(context.Start, context.Stop, replacements);
        }

        protected void AddMessage(ParserRuleContext context, CompilerMessageLevel level, CompilerMessageCode code, params object[] args)
        {
            AddMessage(CreateMessage(level, code, context.Start, context.Stop, args));
        }

        protected void AddMessage(PositionalElement element, CompilerMessageLevel level, CompilerMessageCode code, params object[] args)
        {
            AddMessage(CreateMessage(level, code, element.SourceLine, element.SourceColumn, element.SourceLine, element.EndColumn, args));
        }

        protected void AddMessageStoppingAtPrecedingToken(ParserRuleContext context, CompilerMessageLevel level, CompilerMessageCode code, params object[] args)
        {
            AddMessage(CreateMessage(level, code, context.Start, tokenStream.GetPrecedingToken(context.Stop), args));
        }

        protected void AddMessage(ITerminalNode context, CompilerMessageLevel level, CompilerMessageCode code, params object[] args)
        {
            AddMessage(CreateMessage(level, code, context.Symbol, context.Symbol, args));
        }

        protected void AddMessage(CompilerMessageLevel level, CompilerMessageCode code, IToken start, IToken stop, params object[] args)
        {
            AddMessage(CreateMessage(level, code, start, stop, args));
        }

        protected void AddMessage(CompilerMessageLevel level, CompilerMessageCode code, int lineStart, int colStart, int lineEnd, int colEnd, params object[] args)
        {
            var message = CreateMessage(level, code, lineStart, colStart, lineEnd, colEnd, args);

            AddMessage(message);
        }

        protected CompilerMessage CreateMessage(ParserRuleContext context, CompilerMessageLevel level, CompilerMessageCode code, params object[] args)
            => CreateMessage(level, code, context.Start, context.Stop, args);

        protected CompilerMessage CreateMessage(CompilerMessageLevel level, CompilerMessageCode code, IToken start, IToken stop, params object[] args)
            => CompilerMessageFactory.Create(sourceName, level, code, start, stop, args);

        protected CompilerMessage CreateMessage(CompilerMessageLevel level, CompilerMessageCode code, int lineStart, int colStart, int lineEnd, int colEnd, params object[] args)
            => CompilerMessageFactory.Create(sourceName, level, code, lineStart, colStart, lineEnd, colEnd, args);

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

        protected TElement PositionalLineInfo<TElement>(TElement element, ParserRuleContext ctxt)
            where TElement : PositionalElement
        {
            return PositionalLineInfo(element, ctxt.Start, ctxt.Stop);
        }

        protected TElement PositionalLineInfo<TElement>(TElement element, ITerminalNode ctxt)
            where TElement : PositionalElement
        {
            return PositionalLineInfo(element, ctxt.Symbol, ctxt.Symbol);
        }

        protected TElement PositionalLineInfo<TElement>(TElement element, IToken start, IToken stop)
            where TElement : PositionalElement
        {
            element.SourceLine = start.Line;
            element.SourceColumn = start.Column + 1;
            element.EndColumn = stop.Column + (stop.StopIndex - stop.StartIndex) + 1;

            return element;
        }

        protected TElement LineInfo<TElement>(TElement element, ParserRuleContext ctxt)
            where TElement : BuiltElement
        {
            element.SourceLine = ctxt.Start.Line;
            element.SourceColumn = ctxt.Start.Column + 1;

            return element;
        }

        protected TElement LineInfo<TElement>(TElement element, ITerminalNode ctxt)
            where TElement : BuiltElement
        {
            element.SourceLine = ctxt.Symbol.Line;
            element.SourceColumn = ctxt.Symbol.Column + 1;

            return element;
        }

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
