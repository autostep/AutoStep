using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using AutoStep.Elements.Interaction;
using AutoStep.Elements.Parts;

namespace AutoStep.Language.Interaction.Visitors
{
    using static AutoStep.Language.Interaction.Parser.AutoStepInteractionsParser;

    /// <summary>
    /// Provides the visitor for trait definitions.
    /// </summary>
    internal class TraitDefinitionVisitor : InteractionDefinitionVisitor<TraitDefinitionElement>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TraitDefinitionVisitor"/> class.
        /// </summary>
        /// <param name="sourceName">The source name.</param>
        /// <param name="tokenStream">The token stream.</param>
        /// <param name="rewriter">The token rewriter.</param>
        public TraitDefinitionVisitor(string? sourceName, ITokenStream tokenStream, TokenStreamRewriter rewriter)
            : base(sourceName, tokenStream, rewriter)
        {
        }

        /// <inheritdoc/>
        public override TraitDefinitionElement VisitTraitDefinition([NotNull] TraitDefinitionContext context)
        {
            var refList = context.traitRefList();

            var actualRefs = new HashSet<string>();
            var traits = new List<NameRefElement>();

            foreach (var traitRef in refList.NAME_REF())
            {
                var traitText = traitRef.GetText();

                if (actualRefs.Contains(traitText))
                {
                    // Warning, duplicate not allowed.
                    MessageSet.Add(traitRef, CompilerMessageLevel.Warning, CompilerMessageCode.InteractionDuplicateTrait);
                }
                else
                {
                    var traitNamePart = new NameRefElement(traitText);
                    traitNamePart.AddPositionalLineInfo(traitRef);

                    actualRefs.Add(traitNamePart.Name);
                    traits.Add(traitNamePart);
                }
            }

            // Use TokenStream.GetText to ensure we capture any whitespace.
            Result = new TraitDefinitionElement(TokenStream.GetText(refList), traits);
            Result.AddLineInfo(context);

            VisitChildren(context);

            return Result!;
        }

        /// <inheritdoc/>
        public override TraitDefinitionElement VisitTraitName(TraitNameContext context)
        {
            ProvideName(GetTextFromStringToken(context.STRING()), context);

            return Result!;
        }

        /// <inheritdoc/>
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
