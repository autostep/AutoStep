using Antlr4.Runtime;
using AutoStep.Elements.Interaction;
using AutoStep.Language.Interaction.Parser;
using AutoStep.Language.Position;

namespace AutoStep.Language.Interaction.Visitors
{
    using static AutoStepInteractionsParser;

    /// <summary>
    /// Defines the visitor for generating in-file method definitions.
    /// </summary>
    internal class MethodDefinitionVisitor : InteractionCallChainDeclarationVisitor<MethodDefinitionElement>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MethodDefinitionVisitor"/> class.
        /// </summary>
        /// <param name="sourceName">The source name.</param>
        /// <param name="tokenStream">The token stream.</param>
        /// <param name="rewriter">The stream rewriter.</param>
        /// <param name="compilerOptions">The compiler options.</param>
        /// <param name="positionIndex">The position index (or null if not in use).</param>
        public MethodDefinitionVisitor(string? sourceName, ITokenStream tokenStream, TokenStreamRewriter rewriter, InteractionsCompilerOptions compilerOptions, PositionIndex? positionIndex)
            : base(sourceName, tokenStream, rewriter, compilerOptions, positionIndex)
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

            var nameRef = decl.NAME_REF();

            Result = new MethodDefinitionElement(nameRef.GetText());

            Result.AddPositionalLineInfoExcludingErrorStopToken(decl, TokenStream, METHOD_STRING_ERRNL);
            Result.SourceName = SourceName;

            PositionIndex?.PushScope(Result, context);
            PositionIndex?.AddLineToken(nameRef, LineTokenCategory.InteractionName, LineTokenSubCategory.InteractionMethod);

            VisitChildren(context);

            Result.Documentation = GetDocumentationBlockForElement(context);

            if (context.NEEDS_DEFINING() is object)
            {
                PositionIndex?.AddLineToken(context.NEEDS_DEFINING(), LineTokenCategory.InteractionNeedsDefining);

                // Method is marked as needs defining
                Result.NeedsDefining = true;
            }

            PositionIndex?.PopScope(context);

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

                PositionIndex?.AddLineToken(defArg, LineTokenCategory.InteractionArguments, LineTokenSubCategory.Declaration);

                Result!.Arguments.Add(defArg);
            }

            return Result!;
        }
    }
}
