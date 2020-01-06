using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    internal class StepReferenceVisitor : ArgumentHandlingVisitor<StepReferenceElement>
    {
        private Func<ParserRuleContext, string, CompilerMessage?>? insertionNameValidator;

        protected readonly (int TokenType, string Replace)[] argReplacements = new[]
        {
            (AutoStepParser.ARG_ESCAPE_QUOTE, "'"),
            (AutoStepParser.ARG_EXAMPLE_START_ESCAPE, "<"),
            (AutoStepParser.ARG_EXAMPLE_END_ESCAPE, ">"),
        };

        public StepReferenceVisitor(string? sourceName, ITokenStream tokenStream, Func<ParserRuleContext, string, CompilerMessage?> insertionNameValidator = null)
            : base(sourceName, tokenStream)
        {
            this.insertionNameValidator = insertionNameValidator;
        }

        public StepReferenceVisitor(string? sourceName, ITokenStream tokenStream, TokenStreamRewriter rewriter, Func<ParserRuleContext, string, CompilerMessage?> insertionNameValidator)
            : base(sourceName, tokenStream, rewriter)
        {
            this.insertionNameValidator = insertionNameValidator;
        }

        public StepReferenceElement? BuildStep(StepType type, ParserRuleContext? statementLineContext, AutoStepParser.StatementBodyContext bodyContext)
        {
            var step = new StepReferenceElement
            {
                Type = type,
                RawText = bodyContext.GetText(),
            };

            Result = step;

            LineInfo(step, statementLineContext ?? bodyContext);

            VisitChildren(bodyContext);

            return Result;
        }

        /// <summary>
        /// Visits a float statement argument.
        /// </summary>
        /// <param name="context">The parse context.</param>
        /// <returns>The file.</returns>
        public override StepReferenceElement VisitArgFloat([NotNull] AutoStepParser.ArgFloatContext context)
        {
            Debug.Assert(Result is object);

            var valueText = context.ARG_FLOAT().GetText();
            var symbolText = context.ARG_CURR_SYMBOL()?.GetText();
            var content = symbolText + valueText;

            Result.AddArgument(PositionalLineInfo(
                new StepArgumentElement
                {
                    RawArgument = content,
                    Type = ArgumentType.NumericDecimal,
                    EscapedArgument = content,
                    Value = decimal.Parse(valueText, NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands, CultureInfo.CurrentCulture),
                    Symbol = symbolText,
                }, context));

            return Result;
        }

        /// <summary>
        /// Visits an integer statement argument.
        /// </summary>
        /// <param name="context">The parse context.</param>
        /// <returns>The file.</returns>
        public override StepReferenceElement VisitArgInt([NotNull] AutoStepParser.ArgIntContext context)
        {
            Debug.Assert(Result is object);

            var valueText = context.ARG_INT().GetText();
            var symbolText = context.ARG_CURR_SYMBOL()?.GetText();
            var content = symbolText + valueText;

            Result.AddArgument(PositionalLineInfo(
                new StepArgumentElement
                {
                    RawArgument = content,
                    Type = ArgumentType.NumericInteger,
                    EscapedArgument = content,
                    Value = int.Parse(valueText, NumberStyles.AllowThousands, CultureInfo.CurrentCulture),
                    Symbol = symbolText,
                }, context));

            return Result;
        }


        /// <summary>
        /// Visits a text statement argument.
        /// </summary>
        /// <param name="context">The parse context.</param>
        /// <returns>The file.</returns>
        public override StepReferenceElement VisitArgText([NotNull] AutoStepParser.ArgTextContext context)
        {
            Debug.Assert(Result is object);

            var contentBlock = context.statementArgument();

            Visit(contentBlock);

            PersistWorkingTextSection(argReplacements);

            var escaped = rewriter.GetText(contentBlock.SourceInterval);

            var arg = new StepArgumentElement
            {
                RawArgument = contentBlock.GetText(),
                Type = ArgumentType.Text,

                // The rewriter will contain any modifications that replace the escaped characters.
                EscapedArgument = escaped,
            };

            arg.ReplaceSections(currentArgumentSections);

            if (canArgumentValueBeDetermined)
            {
                arg.Value = escaped;
            }

            currentArgumentSections.Clear();
            canArgumentValueBeDetermined = true;

            Result.AddArgument(PositionalLineInfo(arg, context));

            return Result;
        }

        /// <summary>
        /// Visits an interpolated statement argument.
        /// </summary>
        /// <param name="context">The parse context.</param>
        /// <returns>The file.</returns>
        public override StepReferenceElement VisitArgInterpolate([NotNull] AutoStepParser.ArgInterpolateContext context)
        {
            Debug.Assert(Result is object);

            var contentBlock = context.statementArgument();

            Visit(contentBlock);

            PersistWorkingTextSection(argReplacements);

            var escaped = rewriter.GetText(contentBlock.SourceInterval);

            var arg = new StepArgumentElement
            {
                RawArgument = contentBlock.GetText(),
                Type = ArgumentType.Interpolated,

                // The rewriter will contain any modifications that replace the escaped characters.
                EscapedArgument = escaped,
            };

            arg.ReplaceSections(currentArgumentSections);

            currentArgumentSections.Clear();
            canArgumentValueBeDetermined = true;

            Result.AddArgument(PositionalLineInfo(arg, context));

            return Result;
        }

        public override StepReferenceElement VisitStatementSectionPart([NotNull] AutoStepParser.StatementSectionPartContext context)
        {
            Debug.Assert(Result is object);

            // This will contain literal text.
            Result.AddMatchingText(context.GetText());

            return Result;
        }

        public override StepReferenceElement VisitStatementWs([NotNull] AutoStepParser.StatementWsContext context)
        {
            Debug.Assert(Result is object);

            Result.AddMatchingText(context.GetText());

            return Result;
        }

        /// <summary>
        /// Visits an empty statement argument.
        /// </summary>
        /// <param name="context">The parse context.</param>
        /// <returns>The file.</returns>
        public override StepReferenceElement VisitArgEmpty([NotNull] AutoStepParser.ArgEmptyContext context)
        {
            Debug.Assert(Result is object);

            Result.AddArgument(PositionalLineInfo(
                new StepArgumentElement
                {
                    RawArgument = string.Empty,
                    Type = ArgumentType.Empty,
                    EscapedArgument = string.Empty,
                    Value = string.Empty,
                }, context));

            return Result;
        }

        /// <summary>
        /// Visit the example block for example reference variables inside an argument.
        /// </summary>
        /// <param name="context">The parse context.</param>
        /// <returns>The file.</returns>
        public override StepReferenceElement VisitExampleArgBlock([NotNull] AutoStepParser.ExampleArgBlockContext context)
        {
            Debug.Assert(Result != null);

            PersistWorkingTextSection(argReplacements);

            var content = context.GetText();

            var escaped = EscapeText(
                context,
                argReplacements);

            var allBodyInterval = context.argumentExampleNameBody().SourceInterval;

            var insertionName = rewriter.GetText(allBodyInterval);

            var arg = new ArgumentSectionElement
            {
                RawText = content,
                EscapedText = escaped,

                // The insertion name is the escaped name inside the angle brackets
                ExampleInsertionName = insertionName,
            };

            // If we've got an insertion, then the value of an argument cannot be determined at compile time.
            canArgumentValueBeDetermined = false;

            currentArgumentSections.Add(PositionalLineInfo(arg, context));

            if (insertionNameValidator is object)
            {
                var additionalError = insertionNameValidator(context, insertionName);

                if (additionalError is object)
                {
                    AddMessage(additionalError);
                }
            }

            return Result;
        }
    }
}
