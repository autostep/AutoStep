using System;
using AutoStep.Core;
using AutoStep.Core.Elements;

namespace AutoStep.Compiler.Tests.Builders
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
