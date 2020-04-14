using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using AutoStep.Elements.Interaction;
using AutoStep.Elements.Parts;
using AutoStep.Language.Position;

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
        /// <param name="compilerOptions">The compiler options.</param>
        /// <param name="positionIndex">The position index (or null if not in use).</param>
        public TraitDefinitionVisitor(string? sourceName, ITokenStream tokenStream, TokenStreamRewriter rewriter, InteractionsCompilerOptions compilerOptions, PositionIndex? positionIndex)
            : base(sourceName, tokenStream, rewriter, compilerOptions, positionIndex)
        {
        }

        /// <inheritdoc/>
        public override TraitDefinitionElement VisitTraitDefinition([NotNull] TraitDefinitionContext context)
        {
            var declaration = context.traitDefinitionDeclaration();

            var refList = declaration.traitRefList();

            var actualRefs = new HashSet<string>();
            var traits = new List<NameRefElement>();

            // Use TokenStream.GetText to ensure we capture any whitespace.
            Result = new TraitDefinitionElement(TokenStream.GetText(refList));
            Result.AddLineInfo(declaration);

            PositionIndex?.PushScope(Result, context);
            PositionIndex?.AddLineToken(declaration.TRAIT_DEFINITION(), LineTokenCategory.EntryMarker, LineTokenSubCategory.InteractionTrait);

            foreach (var traitRef in refList.NAME_REF())
            {
                var traitText = traitRef.GetText();

                if (actualRefs.Contains(traitText))
                {
                    PositionIndex?.AddLineToken(traitRef, LineTokenCategory.InteractionName, LineTokenSubCategory.InteractionTrait);

                    // Warning, duplicate not allowed.
                    MessageSet.Add(traitRef, CompilerMessageLevel.Warning, CompilerMessageCode.InteractionDuplicateTrait);
                }
                else
                {
                    var traitNamePart = new NameRefElement(traitText);
                    traitNamePart.AddPositionalLineInfo(traitRef);

                    PositionIndex?.AddLineToken(traitNamePart, LineTokenCategory.InteractionName, LineTokenSubCategory.InteractionTrait);

                    actualRefs.Add(traitNamePart.Name);
                    traits.Add(traitNamePart);
                }
            }

            Result.ProvideNameParts(traits);

            VisitChildren(context);

            PositionIndex?.PopScope(context);

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
