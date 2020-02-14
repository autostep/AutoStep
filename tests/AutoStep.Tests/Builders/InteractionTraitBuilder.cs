using System.Collections.Generic;
using AutoStep.Elements.Interaction;

namespace AutoStep.Tests.Builders
{
    public class InteractionTraitBuilder : BaseBuilder<TraitDefinitionElement>, IInteractionEntityBuilder<TraitDefinitionElement>
    {
        private readonly List<NameRefElement> nameParts = new List<NameRefElement>();


        public InteractionTraitBuilder(string name, int line, int column)
        {
            Built = new TraitDefinitionElement();
            Built.Name = Built.Id = name;
            Built.SourceLine = line;
            Built.StartColumn = column;
        }

        public InteractionTraitBuilder NamePart(string name, int column)
        {
            nameParts.Add(new NameRefElement
            {
                Name = name,
                SourceLine = Built.SourceLine,
                StartColumn = column,
                EndColumn = column + name.Length - 1,                
                EndLine = Built.SourceLine,
            });

            Built.SetNameParts(nameParts.ToArray());

            return this;
        }
    }
}
