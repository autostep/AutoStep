using System.Collections.Generic;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using AutoStep.Language.Interaction.Parser;

namespace AutoStep.Language.Interaction
{
    using static AutoStepInteractionsParser;

    /// <summary>
    /// Visitor for interaction step definitions.
    /// </summary>
    internal class InteractionStepDefineLineVisitor : AutoStepInteractionsParserBaseVisitor<bool>
    {
        private readonly List<LineToken> tokenSet;

        /// <summary>
        /// Initializes a new instance of the <see cref="InteractionStepDefineLineVisitor"/> class.
        /// </summary>
        /// <param name="tokenSet">The set of tokens to add to.</param>
        public InteractionStepDefineLineVisitor(List<LineToken> tokenSet)
        {
            this.tokenSet = tokenSet;
        }

        /// <inheritdoc/>
        public override bool VisitStepDefinition(StepDefinitionContext context)
        {
            AddLineToken(context.STEP_DEFINE(), LineTokenCategory.EntryMarker, LineTokenSubCategory.StepDefine);

            VisitChildren(context);

            return true;
        }

        /// <inheritdoc/>
        public override bool VisitDeclareGiven(DeclareGivenContext context)
        {
            AddLineToken(context.DEF_GIVEN(), LineTokenCategory.StepTypeKeyword, LineTokenSubCategory.Given);

            VisitChildren(context);

            return true;
        }

        /// <inheritdoc/>
        public override bool VisitDeclareWhen(DeclareWhenContext context)
        {
            AddLineToken(context.DEF_WHEN(), LineTokenCategory.StepTypeKeyword, LineTokenSubCategory.When);

            VisitChildren(context);

            return true;
        }

        /// <inheritdoc/>
        public override bool VisitDeclarationArgument(DeclarationArgumentContext context)
        {
            AddLineToken(context, LineTokenCategory.BoundArgument, LineTokenSubCategory.Declaration);

            return true;
        }

        /// <inheritdoc/>
        public override bool VisitDeclarationSection(DeclarationSectionContext context)
        {
            var sectionContent = context.stepDeclarationSectionContent();

            if (sectionContent is DeclarationComponentInsertContext)
            {
                AddLineToken(sectionContent, LineTokenCategory.Placeholder, LineTokenSubCategory.InteractionComponentPlaceholder);
            }
            else if (!(sectionContent is DeclarationWsContext))
            {
                AddLineToken(sectionContent, LineTokenCategory.StepText, LineTokenSubCategory.Declaration);
            }

            return true;
        }

        private void AddLineToken(ITerminalNode node, LineTokenCategory category, LineTokenSubCategory subCat = LineTokenSubCategory.None)
        {
            tokenSet.Add(new LineToken(node.Symbol.Column, category, subCat));
        }

        private void AddLineToken(ParserRuleContext context, LineTokenCategory category, LineTokenSubCategory subCat = LineTokenSubCategory.None)
        {
            tokenSet.Add(new LineToken(context.Start.Column, category, subCat));
        }
    }
}
