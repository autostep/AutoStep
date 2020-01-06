using System.Collections.Generic;
using System.Diagnostics;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using AutoStep.Compiler.Parser;
using AutoStep.Elements;

namespace AutoStep.Compiler
{
    /// <summary>
    /// Base class for visitors that need to handle arguments (tables or statement visitors).
    /// </summary>
    /// <typeparam name="TResult">The result of the visitor.</typeparam>
    internal abstract class ArgumentHandlingVisitor<TResult> : BaseAutoStepVisitor<TResult>
        where TResult : class
    {
        private IToken? textSectionTokenStart;
        private IToken? textSectionTokenEnd;

        /// <summary>
        /// Initializes a new instance of the <see cref="ArgumentHandlingVisitor{TResult}"/> class.
        /// </summary>
        /// <param name="sourceName">The source name.</param>
        /// <param name="tokenStream">The token stream.</param>
        public ArgumentHandlingVisitor(string? sourceName, ITokenStream tokenStream)
            : base(sourceName, tokenStream)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArgumentHandlingVisitor{TResult}"/> class.
        /// </summary>
        /// <param name="sourceName">The source name.</param>
        /// <param name="tokenStream">The token stream.</param>
        /// <param name="rewriter">A shared token rewriter.</param>
        public ArgumentHandlingVisitor(string? sourceName, ITokenStream tokenStream, TokenStreamRewriter rewriter)
            : base(sourceName, tokenStream, rewriter)
        {
        }

        /// <summary>
        /// Gets the set of current argument sections for the argument.
        /// </summary>
        protected List<ArgumentSectionElement> CurrentArgumentSections { get; } = new List<ArgumentSectionElement>();

        /// <summary>
        /// Gets or sets a value indicating whether the argument value can be explicitly determined.
        /// </summary>
        protected bool CanArgumentValueBeDetermined { get; set; } = true;

        /// <summary>
        /// Visit the argument block for text.
        /// </summary>
        /// <param name="context">The parse context.</param>
        /// <returns>The file.</returns>
        public override TResult VisitTextArgBlock([NotNull] AutoStepParser.TextArgBlockContext context)
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
        public override TResult VisitTextCellBlock([NotNull] AutoStepParser.TextCellBlockContext context)
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

        /// <summary>
        /// Invoke this method to add a new text argument section based on the known text positions (and reset for the next position).
        /// </summary>
        /// <param name="replacements">A set of replacements to use for escaping the text values.</param>
        protected void PersistWorkingTextSection(params (int Token, string Replacement)[] replacements)
        {
            if (textSectionTokenStart is object)
            {
                var content = TokenStream.GetText(textSectionTokenStart, textSectionTokenEnd!);

                var escaped = EscapeText(
                    textSectionTokenStart,
                    textSectionTokenEnd!,
                    replacements);

                var arg = new ArgumentSectionElement
                {
                    RawText = content,
                    EscapedText = escaped,
                };

                CurrentArgumentSections.Add(PositionalLineInfo(arg, textSectionTokenStart, textSectionTokenEnd!));
            }

            textSectionTokenStart = null;
            textSectionTokenEnd = null;
        }
    }
}
