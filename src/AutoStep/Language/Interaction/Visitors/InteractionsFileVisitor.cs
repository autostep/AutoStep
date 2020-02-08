using System;
using System.Collections.Generic;
using System.Text;
using Antlr4.Runtime;
using AutoStep.Elements.Interaction;
using AutoStep.Language.Interaction.Parser;

namespace AutoStep.Language.Interaction.Visitors
{
    internal class InteractionsFileVisitor : BaseAutoStepInteractionVisitor<InteractionFileElement>
    {
        public InteractionsFileVisitor(string? sourceName, ITokenStream tokenStream)
            : base(sourceName, tokenStream)
        {
        }

        public InteractionsFileVisitor(string? sourceName, ITokenStream tokenStream, TokenStreamRewriter rewriter)
            : base(sourceName, tokenStream, rewriter)
        {
        }

        public override InteractionFileElement VisitFile(AutoStepInteractionsParser.FileContext context)
        {
            Result = new InteractionFileElement
            {
                SourceName = SourceName
            };

            return Result;
        }

        public override InteractionFileElement VisitTraitDefinition(AutoStepInteractionsParser.TraitDefinitionContext context)
        {
            return null;
        }
    }
}
