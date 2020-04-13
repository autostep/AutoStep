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
    /// Provides the visitor for component definitions.
    /// </summary>
    internal class ComponentDefinitionVisitor : InteractionDefinitionVisitor<ComponentDefinitionElement>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ComponentDefinitionVisitor"/> class.
        /// </summary>
        /// <param name="sourceName">The name of the source.</param>
        /// <param name="tokenStream">The token stream.</param>
        /// <param name="rewriter">The token rewriter.</param>
        /// <param name="compilerOptions">The compiler options.</param>
        /// <param name="positionIndex">The position index (or null if not in use).</param>
        public ComponentDefinitionVisitor(string? sourceName, ITokenStream tokenStream, TokenStreamRewriter rewriter, InteractionsCompilerOptions compilerOptions, PositionIndex? positionIndex)
            : base(sourceName, tokenStream, rewriter, compilerOptions, positionIndex)
        {
        }

        /// <inheritdoc/>
        public override ComponentDefinitionElement VisitComponentDefinition([NotNull] ComponentDefinitionContext context)
        {
            var declaration = context.componentDefinitionDeclaration();

            var componentName = declaration.NAME_REF();

            Result = new ComponentDefinitionElement(componentName.GetText());
            Result.AddPositionalLineInfo(declaration);

            PositionIndex?.PushScope(Result, context);

            PositionIndex?.AddLineToken(declaration.COMPONENT_DEFINITION(), LineTokenCategory.EntryMarker, LineTokenSubCategory.InteractionComponent);
            PositionIndex?.AddLineToken(componentName, LineTokenCategory.InteractionName, LineTokenSubCategory.InteractionComponent);

            VisitChildren(context);

            PositionIndex?.PopScope(context);

            // Apply the known name to each step.
            foreach (var step in Result.Steps)
            {
                step.FixedComponentName = Result.Name;
            }

            return Result;
        }

        /// <inheritdoc/>
        protected override bool ValidateAddedStepDefinition(InteractionStepDefinitionElement stepDef, StepDefinitionBodyContext bodyContext)
        {
            if (stepDef.Parts.OfType<PlaceholderMatchPart>().Any())
            {
                // Component step definitions cannot have a component marker.
                MessageSet.Add(bodyContext.stepDefinition(), CompilerMessageLevel.Error, CompilerMessageCode.InteractionComponentStepDefinitionCannotHaveComponentMarker);

                return false;
            }

            return base.ValidateAddedStepDefinition(stepDef, bodyContext);
        }

        /// <inheritdoc/>
        public override ComponentDefinitionElement VisitComponentName([NotNull] ComponentNameContext context)
        {
            var str = context.STRING();

            PositionIndex?.AddLineToken(context.NAME_KEYWORD(), LineTokenCategory.InteractionPropertyName, LineTokenSubCategory.InteractionName);
            PositionIndex?.AddLineToken(context.STRING(), LineTokenCategory.InteractionString, LineTokenSubCategory.InteractionName);

            if (str is object)
            {
                ProvideName(GetTextFromStringToken(str), context);
            }

            return Result!;
        }

        /// <inheritdoc/>
        public override ComponentDefinitionElement VisitComponentInherits([NotNull] ComponentInheritsContext context)
        {
            var inherits = context.NAME_REF();

            if (inherits is object)
            {
                var inheritsElement = new NameRefElement(inherits.GetText()).AddPositionalLineInfo(inherits);

                PositionIndex?.AddLineToken(context.INHERITS_KEYWORD(), LineTokenCategory.InteractionPropertyName, LineTokenSubCategory.InteractionInherits);
                PositionIndex?.AddLineToken(inheritsElement, LineTokenCategory.InteractionString, LineTokenSubCategory.InteractionName);

                Result!.Inherits = inheritsElement;

                PositionIndex?.PopScope(context);
            }

            return Result!;
        }

        /// <inheritdoc/>
        public override ComponentDefinitionElement VisitComponentTraits(ComponentTraitsContext context)
        {
            var actualRefs = new HashSet<string>();

            PositionIndex?.AddLineToken(context.TRAITS_KEYWORD(), LineTokenCategory.InteractionPropertyName, LineTokenSubCategory.InteractionTrait);

            foreach (var traitRef in context.NAME_REF())
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

                    PositionIndex?.AddLineToken(traitNamePart, LineTokenCategory.InteractionPropertyName, LineTokenSubCategory.InteractionTrait);

                    actualRefs.Add(traitNamePart.Name);
                    Result!.Traits.Add(traitNamePart);
                }
            }

            return Result!;
        }
    }
}
