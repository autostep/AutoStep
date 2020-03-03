using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using AutoStep.Elements.Interaction;
using AutoStep.Elements.Parts;

namespace AutoStep.Language.Interaction.Visitors
{
    using static AutoStep.Language.Interaction.Parser.AutoStepInteractionsParser;

    internal class ComponentDefinitionVisitor : InteractionDefinitionVisitor<ComponentDefinitionElement>
    {
        public ComponentDefinitionVisitor(string? sourceName, ITokenStream tokenStream, TokenStreamRewriter rewriter)
            : base(sourceName, tokenStream, rewriter)
        {
        }

        public override ComponentDefinitionElement VisitComponentDefinition([NotNull] ComponentDefinitionContext context)
        {
            Result = new ComponentDefinitionElement();
            Result.AddLineInfo(context);

            var componentName = context.NAME_REF();

            Result.Id = Result.Name = componentName.GetText();

            VisitChildren(context);

            // Apply the known name to each step.
            foreach (var step in Result.Steps)
            {
                step.FixedComponentName = Result.Name;
            }

            return Result;
        }

        protected override bool ValidateAddedStepDefinition(InteractionStepDefinitionElement stepDef, StepDefinitionBodyContext bodyContext)
        {
            if (stepDef.Parts.OfType<PlaceholderMatchPart>().Any())
            {
                // Component step definitions cannot have a component marker.
                MessageSet.Add(bodyContext.stepDefinition(), CompilerMessageLevel.Error, CompilerMessageCode.InteractionComponentStepDefinitionCannotHaveComponentMarker);

                return false;
            }

            return true;
        }

        public override ComponentDefinitionElement VisitComponentName([NotNull] ComponentNameContext context)
        {
            ProvideName(GetTextFromStringToken(context.STRING()), context);

            return Result!;
        }

        public override ComponentDefinitionElement VisitComponentInherits([NotNull] ComponentInheritsContext context)
        {
            var inherits = context.NAME_REF();

            if (inherits is object)
            {
                var inheritsElement = new NameRefElement
                {
                    Name = inherits.GetText(),
                }.AddPositionalLineInfo(context);

                Result!.Inherits = inheritsElement;
            }

            return Result!;
        }

        public override ComponentDefinitionElement VisitComponentTraits(ComponentTraitsContext context)
        {
            Result!.Traits.AddRange(context.NAME_REF().Select(n => new NameRefElement { Name = n.GetText() }.AddPositionalLineInfo(n)));

            return Result;
        }
    }
}
