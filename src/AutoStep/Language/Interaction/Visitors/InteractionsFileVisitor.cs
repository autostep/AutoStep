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
        private TraitDefinitionVisitor traitVisitor;
        private ComponentDefinitionVisitor componentVisitor;

        public InteractionsFileVisitor(string? sourceName, ITokenStream tokenStream)
            : base(sourceName, tokenStream)
        {
            traitVisitor = new TraitDefinitionVisitor(sourceName, tokenStream, Rewriter);
            componentVisitor = new ComponentDefinitionVisitor(sourceName, tokenStream, Rewriter);
        }

        public override InteractionFileElement VisitFile(AutoStepInteractionsParser.FileContext context)
        {
            Result = new InteractionFileElement
            {
                SourceName = SourceName
            };

            VisitChildren(context);

            return Result;
        }

        public override InteractionFileElement VisitTraitDefinition(AutoStepInteractionsParser.TraitDefinitionContext context)
        {
            var trait = traitVisitor.Build(context);

            MergeVisitorAndReset(traitVisitor);

            if (trait is object)
            {
                // Capture the trait in the graph.
                Result!.TraitGraph.AddOrExtendTrait(trait);
            }

            return Result!;
        }

        public override InteractionFileElement VisitComponentDefinition(AutoStepInteractionsParser.ComponentDefinitionContext context)
        {
            var component = componentVisitor.Build(context);

            MergeVisitorAndReset(componentVisitor);

            if (component is object)
            {
                Result!.Components.Add(component);
            }

            return base.VisitComponentDefinition(context);
        }
    }
}
