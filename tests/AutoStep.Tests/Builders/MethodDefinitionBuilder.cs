using AutoStep.Elements.Interaction;

namespace AutoStep.Tests.Builders
{
    public class MethodDefinitionBuilder : InteractionMethodCallChainBuilder<MethodDefinitionElement>
    {
        public MethodDefinitionBuilder(MethodDefinitionElement element) : base(element)
        {
        }

        public MethodDefinitionBuilder Argument(string name, int line, int column)
        {
            Built.Arguments.Add(new MethodDefinitionArgumentElement
            {
                SourceLine = line,
                StartColumn = column,
                EndLine = line,
                EndColumn = column + name.Length - 1,
                Name = name
            });

            return this;
        }
    }
}
