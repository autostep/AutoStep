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
    internal class ArgumentHandlingVisitor<TElement> : BaseAutoStepVisitor<TElement>
        where TElement : class
    {
        private IToken? textSectionTokenStart;
        private IToken? textSectionTokenEnd;

        protected List<ArgumentSectionElement> currentArgumentSections = new List<ArgumentSectionElement>();
        protected bool canArgumentValueBeDetermined = true;

        public ArgumentHandlingVisitor(string? sourceName, ITokenStream tokenStream)
            : base(sourceName, tokenStream)
        {
        }

        public ArgumentHandlingVisitor(string? sourceName, ITokenStream tokenStream, TokenStreamRewriter rewriter)
            : base(sourceName, tokenStream, rewriter)
        {
        }

        /// <summary>
        /// Visit the argument block for text.
        /// </summary>
        /// <param name="context">The parse context.</param>
        /// <returns>The file.</returns>
        public override TElement VisitTextArgBlock([NotNull] AutoStepParser.TextArgBlockContext context)
        {
            Debug.Assert(Result != null);

            if (textSectionTokenStart is null)
            {
                textSectionTokenStart = context.Start;
            }

            // Move the end
            textSectionTokenEnd = context.Stop;

            return Result;
        }

        /// <summary>
        /// Visit a block within the cell that contains text.
        /// </summary>
        /// <param name="context">The parse context.</param>
        /// <returns>The file.</returns>
        public override TElement VisitTextCellBlock([NotNull] AutoStepParser.TextCellBlockContext context)
        {
            Debug.Assert(Result != null);

            if (textSectionTokenStart is null)
            {
                textSectionTokenStart = context.Start;
            }

            // Move the end of the text section to the stop token.
            textSectionTokenEnd = context.Stop;

            return Result;
        }

        protected void PersistWorkingTextSection(params (int Token, string Replacement)[] replacements)
        {
            if (textSectionTokenStart is object)
            {
                var content = tokenStream.GetText(textSectionTokenStart, textSectionTokenEnd!);

                var escaped = EscapeText(
                    textSectionTokenStart,
                    textSectionTokenEnd!,
                    replacements);

                var arg = new ArgumentSectionElement
                {
                    RawText = content,
                    EscapedText = escaped,
                };

                currentArgumentSections.Add(PositionalLineInfo(arg, textSectionTokenStart, textSectionTokenEnd!));
            }

            textSectionTokenStart = null;
            textSectionTokenEnd = null;
        }
    }
}
