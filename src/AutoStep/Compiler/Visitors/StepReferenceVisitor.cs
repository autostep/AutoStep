using System;
using System.Diagnostics;
using System.Globalization;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using AutoStep.Compiler.Parser;
using AutoStep.Elements;
using AutoStep.Elements.Parts;

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

        private QuotedStringPart? currentQuotedPart = null;
        private InterpolatePart? currentInterpolatePart = null;

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
        /// <param name="statementLineContext">The entire line context .</param>
        /// <returns>A generated step reference.</returns>
        public StepReferenceElement BuildStep(StepType type, AutoStepParser.StatementContext statementContext)
        {
            var step = new StepReferenceElement
            {
                Type = type,
                RawText = statementContext.GetText(),
            };

            Result = step;

            LineInfo(step, statementContext);

            VisitChildren(statementContext);

            return Result;
        }

        public override StepReferenceElement VisitStatementQuotedString([NotNull] AutoStepParser.StatementQuotedStringContext context)
        {
            Debug.Assert(Result != null);

            currentQuotedPart = CreatePart<QuotedStringPart>(context);

            VisitChildren(context);

            Result.AddPart(currentQuotedPart);

            currentQuotedPart = null;
            currentInterpolatePart = null;

            return Result;
        }

        public override StepReferenceElement VisitStatementWord([NotNull] AutoStepParser.StatementWordContext context)
        {
            AddPart(CreatePart<WordPart>(context));

            return Result!;
        }

        public override StepReferenceElement VisitStatementEscapedChar([NotNull] AutoStepParser.StatementEscapedCharContext context)
        {
            var part = CreatePart<WordPart>(context);

            part.EscapedText = EscapeText(context, escapeReplacements);

            AddPart(part);

            return Result!;
        }

        public override StepReferenceElement VisitStatementVariable([NotNull] AutoStepParser.StatementVariableContext context)
        {
            var variablePart = CreatePart<VariablePart>(context);

            variablePart.VariableName = context.statementVariableName().GetText();

            if (insertionNameValidator is object)
            {
                var additionalError = insertionNameValidator(context, variablePart.VariableName);

                if (additionalError is object)
                {
                    AddMessage(additionalError);
                }
            }

            return Result!;
        }

        public override StepReferenceElement VisitStatementInt([NotNull] AutoStepParser.StatementIntContext context)
        {
            var intPart = CreatePart<IntPart>(context);

            intPart.Value = int.Parse(context.STATEMENT_INT().GetText(), NumberStyles.AllowThousands, CultureInfo.CurrentCulture);

            intPart.Symbol = context.STATEMENT_SYMBOL()?.GetText();

            return Result!;
        }

        public override StepReferenceElement VisitStatementFloat([NotNull] AutoStepParser.StatementFloatContext context)
        {
            var floatPart = CreatePart<FloatPart>(context);

            floatPart.Value = decimal.Parse(
                context.STATEMENT_FLOAT().GetText(),
                NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands,
                CultureInfo.CurrentCulture);

            floatPart.Symbol = context.STATEMENT_SYMBOL()?.GetText();

            return Result!;
        }

        public override StepReferenceElement VisitStatementSymbol([NotNull] AutoStepParser.StatementSymbolContext context)
        {
            AddPart(CreatePart<WordPart>(context));

            return Result!;
        }

        public override StepReferenceElement VisitStatementInterpolate([NotNull] AutoStepParser.StatementInterpolateContext context)
        {
            AddPart(CreatePart<InterpolatePart>(context));

            return Result!;
        }

        private void AddPart(ContentPart part)
        {
            if (currentQuotedPart is object)
            {
                if (currentInterpolatePart is null)
                {
                    currentQuotedPart.AddPart(part);
                }
                else
                {
                    currentInterpolatePart.AddPart(part);
                }

                if (part is InterpolatePart interpolated)
                {
                    currentInterpolatePart = interpolated;
                }
            }
            else
            {
                Result!.AddPart(part);
            }
        }
    }
}
