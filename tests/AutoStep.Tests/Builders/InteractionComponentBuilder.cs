using System.Collections.Generic;
using AutoStep.Elements.Interaction;

namespace AutoStep.Tests.Builders
{
    public class InteractionComponentBuilder : BaseBuilder<ComponentDefinitionElement>, IInteractionEntityBuilder<ComponentDefinitionElement>
    {
        private readonly List<NameRefElement> nameParts = new List<NameRefElement>();


        public InteractionComponentBuilder(string name, int line, int column)
        {
            Built = new ComponentDefinitionElement();
            Built.Name = Built.Id = name;
            Built.SourceLine = line;
            Built.StartColumn = column;
        }

        public InteractionComponentBuilder Trait(string name, int line, int column)
        {
            Built.Traits.Add(new NameRefElement
            {
                Name = name,
                SourceLine = line,
                StartColumn = column,
                EndColumn = column + name.Length - 1,
                EndLine = line,
            });

            return this;
        }

        public InteractionComponentBuilder BasedOn(string name, int line, int column)
        {
            Built.BasedOn = new NameRefElement
            {
                Name = name,
                SourceLine = line,
                StartColumn = column,
                EndColumn = column + name.Length - 1,
                EndLine = line,
            };

            return this;
        }
    }
}
