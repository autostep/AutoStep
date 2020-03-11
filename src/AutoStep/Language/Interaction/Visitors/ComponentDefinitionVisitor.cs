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
        public ComponentDefinitionVisitor(string? sourceName, ITokenStream tokenStream, TokenStreamRewriter rewriter)
            : base(sourceName, tokenStream, rewriter)
        {
        }

        /// <inheritdoc/>
        public override ComponentDefinitionElement VisitComponentDefinition([NotNull] ComponentDefinitionContext context)
        {
            var componentName = context.NAME_REF();

            Result = new ComponentDefinitionElement(componentName.GetText());
            Result.AddLineInfo(context);

            VisitChildren(context);

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
            ProvideName(GetTextFromStringToken(context.STRING()), context);

            return Result!;
        }

        /// <inheritdoc/>
        public override ComponentDefinitionElement VisitComponentInherits([NotNull] ComponentInheritsContext context)
        {
            var inherits = context.NAME_REF();

            if (inherits is object)
            {
                var inheritsElement = new NameRefElement(inherits.GetText()).AddPositionalLineInfo(inherits);

                Result!.Inherits = inheritsElement;
            }

            return Result!;
        }

        /// <inheritdoc/>
        public override ComponentDefinitionElement VisitComponentTraits(ComponentTraitsContext context)
        {
            var actualRefs = new HashSet<string>();

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

                    actualRefs.Add(traitNamePart.Name);
                    Result!.Traits.Add(traitNamePart);
                }
            }

            return Result!;
        }
    }
}
