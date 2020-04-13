using AutoStep.Elements.Interaction;

namespace AutoStep.Tests.Builders
{
    internal class MethodDefinitionBuilder : InteractionMethodCallChainBuilder<MethodDefinitionElement>
    {
        public MethodDefinitionBuilder(MethodDefinitionElement element) : base(element)
        {
        }

        public MethodDefinitionBuilder NeedsDefining()
        {
            Built.NeedsDefining = true;
            return this;
        }

        public MethodDefinitionBuilder Argument(string name, int line, int column)
        {
            Built.Arguments.Add(new MethodDefinitionArgumentElement(name)
            {
                SourceLine = line,
                StartColumn = column,
                EndLine = line,
                EndColumn = column + name.Length - 1
            });

            return this;
        }

        public MethodDefinitionBuilder Documentation(string docs)
        {
            Built.Documentation = docs;

            return this;
        }
    }
}
