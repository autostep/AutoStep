using AutoStep.Core;

namespace AutoStep.Compiler.Tests.Builders
{
    public class ArgumentBuilder : BaseBuilder<StepArgument>
    {
        private bool moddedValue = false;

        public ArgumentBuilder(StepReference containingStep, string rawValue, ArgumentType type, int start, int end)
        {
            Built = new StepArgument
            {
                SourceLine = containingStep.SourceLine,
                Type = type,
                RawArgument = rawValue,
                EscapedArgument = rawValue,
                Value = rawValue,
                SourceColumn = start,
                EndColumn = end
            };
        }

        public ArgumentBuilder Unescaped(string value)
        {
            Built.EscapedArgument = value;
            
            if(!moddedValue)
            {
                Built.Value = value;
            }

            return this;
        }

        public ArgumentBuilder Value(int value)
        {
            Built.Value = value;
            moddedValue = true;

            return this;
        }

        public ArgumentBuilder NullValue()
        {
            Built.Value = null;
            moddedValue = true;

            return this;
        }

        public ArgumentBuilder Value(decimal value)
        {
            Built.Value = value; 
            moddedValue = true;

            return this;
        }

        public ArgumentBuilder Symbol(string symbol)
        {
            Built.Symbol = symbol;            

            return this;
        }
    }


}
