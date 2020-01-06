using System;
using AutoStep.Elements;

namespace AutoStep.Tests.Builders
{
    public class StepDefinitionBuilder : BaseBuilder<StepDefinitionElement>, IStepCollectionBuilder<StepDefinitionElement>
    {
        public StepDefinitionBuilder(StepType type, string declaration, int line, int column)
        {
            Built = new StepDefinitionElement
            {
                SourceLine = line,
                SourceColumn = column,
                Type = type,
                Declaration = declaration
            };
        }

        public StepDefinitionBuilder Description(string description)
        {
            Built.Description = description;

            return this;
        }

        public StepDefinitionBuilder Argument(ArgumentType type, string rawValue, int start, int end, Action<ArgumentBuilder> cfg = null)
        {
            var argumentBuilder = new ArgumentBuilder(Built, rawValue, type, start, end);

            if (cfg is object)
            {
                cfg(argumentBuilder);
            }

            Built.AddArgument(argumentBuilder.Built);

            return this;
        }
    }


}
