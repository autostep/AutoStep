using AutoStep.Elements.Interaction;

namespace AutoStep.Tests.Builders
{
    public class InteractionComponentBuilder : BaseBuilder<ComponentDefinitionElement>, IInteractionEntityBuilder<ComponentDefinitionElement>
    {
        public InteractionComponentBuilder(string name, int line, int column)
        {
            Built = new ComponentDefinitionElement(name);
            Built.SourceLine = line;
            Built.StartColumn = column;
        }

        public InteractionComponentBuilder Name(string name)
        {
            Built.Name = name;

            return this;
        }

        public InteractionComponentBuilder Trait(string name, int line, int column)
        {
            Built.Traits.Add(new NameRefElement(name)
            {
                SourceLine = line,
                StartColumn = column,
                EndColumn = column + name.Length - 1,
                EndLine = line,
            });

            return this;
        }

        public InteractionComponentBuilder Inherits(string name, int line, int column)
        {
            Built.Inherits = new NameRefElement(name)
            {
                SourceLine = line,
                StartColumn = column,
                EndColumn = column + name.Length - 1,
                EndLine = line,
            };

            return this;
        }
    }
}
