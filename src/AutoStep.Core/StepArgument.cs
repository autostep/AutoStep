using System;
using System.Collections.Generic;

namespace AutoStep.Core
{
    /// <summary>
    /// Represents a provided argument to a Step Reference.
    /// </summary>
    public class StepArgument : PositionalElement
    {
        /// <summary>
        /// Gets or sets the raw argument text.
        /// </summary>
        public string RawArgument { get; set; }

        /// <summary>
        /// Gets or sets the escaped argument body.
        /// </summary>
        public string EscapedArgument { get; set; }

        /// <summary>
        /// Gets or sets the parsed value of the argument. Null for interpolated arguments.
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// Gets or sets the argument type.
        /// </summary>
        public ArgumentType Type { get; set; }

        /// <summary>
        /// Gets or sets an optional leading symbol from the original value ($, £, etc).
        /// </summary>
        public string Symbol { get; set; }
    }
}
