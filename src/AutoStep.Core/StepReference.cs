using System;
using System.Collections.Generic;

namespace AutoStep.Core
{

    public class StepReference : BuiltElement
    {
        public StepType? BindingType { get; set; }

        public StepType Type { get; set; }

        public string RawText { get; set; }

        public List<StepArgument> Arguments { get; set; }

        public void AddArgument(StepArgument argument)
        {
            if(Arguments == null)
            {
                Arguments = new List<StepArgument>();
            }

            Arguments.Add(argument);
        }
    }

    public enum ArgumentType
    {
        String,
        Int,
        Decimal,
        Interpolated,
        Empty
    }

    public class StepArgument : PositionalElement
    {
        public string RawArgument { get; set; }

        public string UnescapedArgument { get; set; }

        /// <summary>
        /// Parsed value of the argument. Null for interpolated arguments.
        /// </summary>
        public object Value { get; set; }

        public ArgumentType Type { get; set; }

        /// <summary>
        /// May contain a leading symbol from the original value ($, £, etc).
        /// </summary>
        public string Symbol { get; set; }
    }
}
