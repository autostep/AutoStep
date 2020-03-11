using Antlr4.Runtime;
using AutoStep.Elements.Interaction;
using AutoStep.Language.Interaction.Parser;

namespace AutoStep.Language.Interaction.Visitors
{
    using static AutoStepInteractionsParser;

    /// <summary>
    /// Defines the visitor for generating in-file method definitions.
    /// </summary>
    internal class MethodDefinitionVisitor : InteractionMethodDeclarationVisitor<MethodDefinitionElement>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MethodDefinitionVisitor"/> class.
        /// </summary>
        /// <param name="sourceName">The source name.</param>
        /// <param name="tokenStream">The token stream.</param>
        /// <param name="rewriter">The stream rewriter.</param>
        public MethodDefinitionVisitor(string? sourceName, ITokenStream tokenStream, TokenStreamRewriter rewriter)
            : base(sourceName, tokenStream, rewriter)
        {
        }

        /// <summary>
        /// Create a <see cref="MethodDefinitionElement"/> from the parse tree context.
        /// </summary>
        /// <param name="context">The ANTLR context.</param>
        /// <returns>A new element.</returns>
        public MethodDefinitionElement Build(MethodDefinitionContext context)
        {
            var decl = context.methodDeclaration();

            Result = new MethodDefinitionElement(decl.NAME_REF().GetText());

            Result.AddPositionalLineInfoExcludingErrorStopToken(decl, TokenStream, METHOD_STRING_ERRNL);
            Result.SourceName = SourceName;

            VisitChildren(context);

            if (context.NEEDS_DEFINING() is object)
            {
                // Method is marked as needs defining
                Result.NeedsDefining = true;
            }

            return Result;
        }

        /// <inheritdoc/>
        public override MethodDefinitionElement VisitMethodDefArgs(MethodDefArgsContext context)
        {
            foreach (var argName in context.PARAM_NAME())
            {
                // Don't consume error nodes.
                if (argName is Antlr4.Runtime.Tree.ErrorNodeImpl)
                {
                    continue;
                }

                var defArg = new MethodDefinitionArgumentElement(argName.GetText()).AddPositionalLineInfo(argName);

                Result!.Arguments.Add(defArg);
            }

            return Result!;
        }
    }
}
