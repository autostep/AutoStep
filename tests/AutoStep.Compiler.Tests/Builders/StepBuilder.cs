using System;
using AutoStep.Core;

namespace AutoStep.Compiler.Tests.Builders
{
    public class StepBuilder : BaseBuilder<UnknownStepReference>
    {
        public StepBuilder(string body, StepType type, StepType? bindingType, int line, int column)
        {
            Built = new UnknownStepReference
            {
                Type = type,
                BindingType = bindingType,
                SourceLine = line,
                SourceColumn = column,
                RawText = body
            };
        }

        public StepBuilder Argument(ArgumentType type, string rawValue, int start, int end, Action<ArgumentBuilder> cfg = null)
        {
            var argumentBuilder = new ArgumentBuilder(Built, rawValue, type, start, end);

            if(cfg is object)
            {
                cfg(argumentBuilder);
            }

            Built.AddArgument(argumentBuilder.Built);

            return this;
        }
    }


}
