using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using AutoStep.Elements.Interaction;

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

            return Result;
        }

        public override ComponentDefinitionElement VisitComponentName([NotNull] ComponentNameContext context)
        {
            ProvideName(GetTextFromStringToken(context.STRING()), context);

            return Result!;
        }

        public override ComponentDefinitionElement VisitComponentBasedOn([NotNull] ComponentBasedOnContext context)
        {
            var basedOn = context.NAME_REF();

            if (basedOn is object)
            {
                var basedOnElement = new NameRefElement
                {
                    Name = basedOn.GetText(),
                }.AddPositionalLineInfo(context);

                Result!.BasedOn = basedOnElement;
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
