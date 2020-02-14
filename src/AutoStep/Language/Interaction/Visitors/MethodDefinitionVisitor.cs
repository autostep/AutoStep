using System;
using System.Collections.Generic;
using System.Text;
using Antlr4.Runtime;
using AutoStep.Elements.Interaction;
using AutoStep.Language.Interaction.Parser;

namespace AutoStep.Language.Interaction.Visitors
{
    using static AutoStepInteractionsParser;

    internal class MethodDefinitionVisitor : InteractionMethodDeclarationVisitor<MethodDefinitionElement>
    {
        public MethodDefinitionVisitor(string? sourceName, ITokenStream tokenStream, TokenStreamRewriter rewriter)
            : base(sourceName, tokenStream, rewriter)
        {
        }

        public MethodDefinitionElement Build(MethodDefinitionContext context)
        {
            Result = new MethodDefinitionElement();

            var decl = context.methodDeclaration();

            Result.AddPositionalLineInfo(decl);

            Result.Name = decl.NAME_REF().GetText();

            VisitChildren(context);

            return Result;
        }

        public override MethodDefinitionElement VisitMethodDefArgs(MethodDefArgsContext context)
        {
            foreach (var argName in context.PARAM_NAME())
            {
                var defArg = new MethodDefinitionArgumentElement().AddPositionalLineInfo(argName);
                defArg.Name = argName.GetText();

                Result!.Arguments.Add(defArg);
            }

            return Result;
        }
    }
}
