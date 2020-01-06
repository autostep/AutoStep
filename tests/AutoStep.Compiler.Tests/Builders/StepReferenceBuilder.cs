using System;
using AutoStep.Core;
using AutoStep.Core.Elements;

namespace AutoStep.Compiler.Tests.Builders
{
    public class StepReferenceBuilder : BaseBuilder<StepReferenceElement>
    {
        public StepReferenceBuilder(string body, StepType type, StepType? bindingType, int line, int column)
        {
            Built = new StepReferenceElement
            {
                Type = type,
                BindingType = bindingType,
                SourceLine = line,
                SourceColumn = column,
                RawText = body
            };
        }

        public StepReferenceBuilder Argument(ArgumentType type, string rawValue, int start, int end, Action<ArgumentBuilder> cfg = null)
        {
            var argumentBuilder = new ArgumentBuilder(Built, rawValue, type, start, end);

            if(cfg is object)
            {
                cfg(argumentBuilder);
            }

            Built.AddArgument(argumentBuilder.Built);

            return this;
        }

        public StepReferenceBuilder Table(int line, int column, Action<TableBuilder> cfg)
        {
            var tableBuilder = new TableBuilder(line, column);

            cfg(tableBuilder);

            Built.Table = tableBuilder.Built;

            return this;
        }

        public StepReferenceBuilder MatchingText(params string[] parts)
        {
            foreach (var part in parts)
            {
                Built.AddMatchingText(part);
            }

            return this;
        }
    }
}
