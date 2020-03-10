using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using AutoStep.Elements.Interaction;
using AutoStep.Elements.Parts;

namespace AutoStep.Language.Interaction.Visitors
{
    using static AutoStep.Language.Interaction.Parser.AutoStepInteractionsParser;

    internal class TraitDefinitionVisitor : InteractionDefinitionVisitor<TraitDefinitionElement>
    {
        public TraitDefinitionVisitor(string? sourceName, ITokenStream tokenStream, TokenStreamRewriter rewriter)
            : base(sourceName, tokenStream, rewriter)
        {
        }

        public override TraitDefinitionElement VisitTraitDefinition([NotNull] TraitDefinitionContext context)
        {
            var refList = context.traitRefList();

            var actualRefs = new HashSet<NameRefElement>();

            foreach (var traitRef in refList.NAME_REF())
            {
                var traitNamePart = new NameRefElement(traitRef.GetText());

                traitNamePart.AddPositionalLineInfo(traitRef);

                if (actualRefs.Contains(traitNamePart))
                {
                    // Warning, duplicate not allowed.
                    MessageSet.Add(traitRef, CompilerMessageLevel.Warning, CompilerMessageCode.InteractionDuplicateTrait);
                }
                else
                {
                    actualRefs.Add(traitNamePart);
                }
            }

            // Use TokenStream.GetText to ensure we capture any whitespace.
            Result = new TraitDefinitionElement(TokenStream.GetText(refList), actualRefs.ToList());
            Result.AddLineInfo(context);

            VisitChildren(context);

            return Result!;
        }

        public override TraitDefinitionElement VisitTraitName(TraitNameContext context)
        {
            ProvideName(GetTextFromStringToken(context.STRING()), context);

            return Result!;
        }

        protected override bool ValidateAddedStepDefinition(InteractionStepDefinitionElement stepDef, StepDefinitionBodyContext parserContext)
        {
            if (!stepDef.Parts.OfType<PlaceholderMatchPart>().Any())
            {
                // Trait step definitions must have at least one component match part, to ensure they
                // can be differentiated from each other later.
                MessageSet.Add(parserContext.stepDefinition(), CompilerMessageLevel.Error, CompilerMessageCode.InteractionTraitStepDefinitionMustHaveComponent);

                return false;
            }

            return base.ValidateAddedStepDefinition(stepDef, parserContext);
        }
    }
}
