using System;
using System.Diagnostics;
using System.Globalization;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using AutoStep.Compiler.Parser;
using AutoStep.Elements;
using AutoStep.Elements.StepTokens;

namespace AutoStep.Compiler
{
    /// <summary>
    /// Handles generating step references from the Antlr parse context.
    /// </summary>
    internal class StepReferenceVisitor : BaseAutoStepVisitor<StepReferenceElement>
    {
        private readonly (int TokenType, string Replace)[] escapeReplacements = new[]
        {
            (AutoStepParser.STATEMENT_ESCAPED_QUOTE, "'"),
            (AutoStepParser.STATEMENT_ESCAPED_DBLQUOTE, "\""),
            (AutoStepParser.STATEMENT_ESCAPED_VARSTART, ">"),
            (AutoStepParser.STATEMENT_ESCAPED_VAREND, "<"),
        };

        private readonly Func<ParserRuleContext, string, CompilerMessage?>? insertionNameValidator;

        private int textColumnOffset = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="StepReferenceVisitor"/> class.
        /// </summary>
        /// <param name="sourceName">The name of the source.</param>
        /// <param name="tokenStream">The token stream.</param>
        /// <param name="insertionNameValidator">The insertion name validator callback, used to check for valid insertion names.</param>
        public StepReferenceVisitor(string? sourceName, ITokenStream tokenStream, Func<ParserRuleContext, string, CompilerMessage?>? insertionNameValidator = null)
            : base(sourceName, tokenStream)
        {
            this.insertionNameValidator = insertionNameValidator;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StepReferenceVisitor"/> class.
        /// </summary>
        /// <param name="sourceName">The name of the source.</param>
        /// <param name="tokenStream">The token stream.</param>
        /// <param name="rewriter">A shared escape rewriter.</param>
        /// <param name="insertionNameValidator">The insertion name validator callback, used to check for valid insertion names.</param>
        public StepReferenceVisitor(string? sourceName, ITokenStream tokenStream, TokenStreamRewriter rewriter, Func<ParserRuleContext, string, CompilerMessage?> insertionNameValidator)
            : base(sourceName, tokenStream, rewriter)
        {
            this.insertionNameValidator = insertionNameValidator;
        }

        /// <summary>
        /// Builds a step, taking the Step Type, and the statement line Antlr context.
        /// </summary>
        /// <param name="type">The step type.</param>
        /// <param name="statementContext">The statement context.</param>
        /// <returns>A generated step reference.</returns>
        public StepReferenceElement BuildStep(StepType type, AutoStepParser.StatementContext statementContext)
        {
            var step = new StepReferenceElement
            {
                Type = type,
            };

            Result = step;

            LineInfo(step, statementContext);

            VisitChildren(statementContext);

            // No more parts, convert to array for performance.
            step.FreezeTokens();

            return Result;
        }

        /// <summary>
        /// Visits the statement body.
        /// </summary>
        /// <param name="context">The parser context.</param>
        /// <returns>The step reference.</returns>
        public override StepReferenceElement VisitStatementBody([NotNull] AutoStepParser.StatementBodyContext context)
        {
            Debug.Assert(Result is object);

            textColumnOffset = context.Start.Column;

            VisitChildren(context);

            Result.RawText = context.GetText();

            return Result;
        }

        /// <summary>
        /// Visits a statement single quote.
        /// </summary>
        /// <param name="context">The parser context.</param>
        /// <returns>The step reference.</returns>
        public override StepReferenceElement VisitStatementQuote([NotNull] AutoStepParser.StatementQuoteContext context)
        {
            AddPart(CreatePart(context, (s, l) => new QuoteToken(false, s)));

            return Result!;
        }

        /// <summary>
        /// Visits a statement colon.
        /// </summary>
        /// <param name="context">The parser context.</param>
        /// <returns>The step reference.</returns>
        public override StepReferenceElement VisitStatementColon([NotNull] AutoStepParser.StatementColonContext context)
        {
            AddPart(CreatePart(context, (s, l) => new TextToken(s, l)));

            return Result!;
        }

        /// <summary>
        /// Visits a double quote.
        /// </summary>
        /// <param name="context">The parser context.</param>
        /// <returns>The step reference.</returns>
        public override StepReferenceElement VisitStatementDoubleQuote([NotNull] AutoStepParser.StatementDoubleQuoteContext context)
        {
            var part = CreatePart(context, (s, l) => new QuoteToken(true, s));

            AddPart(part);

            return Result!;
        }

        /// <summary>
        /// Visits a statement word.
        /// </summary>
        /// <param name="context">The parser context.</param>
        /// <returns>The step reference.</returns>
        public override StepReferenceElement VisitStatementWord([NotNull] AutoStepParser.StatementWordContext context)
        {
            AddPart(CreatePart(context, (s, l) => new TextToken(s, l)));

            return Result!;
        }

        /// <summary>
        /// Visits a statement escaped character.
        /// </summary>
        /// <param name="context">The parser context.</param>
        /// <returns>The step reference.</returns>
        public override StepReferenceElement VisitStatementEscapedChar([NotNull] AutoStepParser.StatementEscapedCharContext context)
        {
            var part = CreatePart(context, (s, l) => new EscapedCharToken(EscapeText(context, escapeReplacements), s, l));

            AddPart(part);

            return Result!;
        }

        /// <summary>
        /// Visits an unmatched variable marker.
        /// </summary>
        /// <param name="context">The parser context.</param>
        /// <returns>The step reference.</returns>
        public override StepReferenceElement VisitStatementVarUnmatched([NotNull] AutoStepParser.StatementVarUnmatchedContext context)
        {
            AddPart(CreatePart(context, (s, l) => new TextToken(s, l)));

            return Result!;
        }

        /// <summary>
        /// Visits a statement variable.
        /// </summary>
        /// <param name="context">The parser context.</param>
        /// <returns>The step reference.</returns>
        public override StepReferenceElement VisitStatementVariable([NotNull] AutoStepParser.StatementVariableContext context)
        {
            var variablePart = CreatePart(context, (s, l) => new VariableToken(context.statementVariableName().GetText(), s, l));

            if (insertionNameValidator is object)
            {
                var additionalError = insertionNameValidator(context, variablePart.VariableName);

                if (additionalError is object)
                {
                    AddMessage(additionalError);
                }
            }

            AddPart(variablePart);

            return Result!;
        }

        /// <summary>
        /// Visits a statement int.
        /// </summary>
        /// <param name="context">The parser context.</param>
        /// <returns>The step reference.</returns>
        public override StepReferenceElement VisitStatementInt([NotNull] AutoStepParser.StatementIntContext context)
        {
            var intPart = CreatePart(context, (s, l) => new IntToken(s, l));

            AddPart(intPart);

            return Result!;
        }

        /// <summary>
        /// Visits a statement float.
        /// </summary>
        /// <param name="context">The parser context.</param>
        /// <returns>The step reference.</returns>
        public override StepReferenceElement VisitStatementFloat([NotNull] AutoStepParser.StatementFloatContext context)
        {
            var floatPart = CreatePart(context, (s, l) => new FloatToken(s, l));

            AddPart(floatPart);

            return Result!;
        }

        /// <summary>
        /// Visits a statement interpolation start.
        /// </summary>
        /// <param name="context">The parser context.</param>
        /// <returns>The step reference.</returns>
        public override StepReferenceElement VisitStatementInterpolate([NotNull] AutoStepParser.StatementInterpolateContext context)
        {
            // Interpolate part itself is just the colon.
            AddPart(CreatePart(context.STATEMENT_COLON(), (s, l) => new InterpolateStartToken(s)));

            // Now add a part for the first word.
            AddPart(CreatePart(context.STATEMENT_WORD(), (s, l) => new TextToken(s, l)));

            return Result!;
        }

        private void AddPart(StepToken part)
        {
            Result!.AddToken(part);
        }

        private TStepPart CreatePart<TStepPart>(ParserRuleContext ctxt, Func<int, int, TStepPart> creator)
            where TStepPart : StepToken
        {
            var offset = textColumnOffset;
            var start = ctxt.Start.Column - offset;
            var startIndex = ctxt.Start.StartIndex;

            var part = creator(start, (ctxt.Stop.StopIndex - startIndex) + 1);

            PositionalLineInfo(part, ctxt);

            return part;
        }

        private TStepPart CreatePart<TStepPart>(ITerminalNode ctxt, Func<int, int, TStepPart> creator)
            where TStepPart : StepToken
        {
            var offset = textColumnOffset;
            var start = ctxt.Symbol.Column - offset;
            var startIndex = ctxt.Symbol.StartIndex;

            var part = creator(start, (ctxt.Symbol.StopIndex - startIndex) + 1);

            PositionalLineInfo(part, ctxt);

            return part;
        }
    }
}
