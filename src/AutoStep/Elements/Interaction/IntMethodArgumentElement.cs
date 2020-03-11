using System;
using System.Collections.Generic;
using System.Text;
using AutoStep.Language;

namespace AutoStep.Elements.Interaction
{
    /// <summary>
    /// Represents a literal integer method argument.
    /// </summary>
    public class IntMethodArgumentElement : MethodArgumentElement
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IntMethodArgumentElement"/> class.
        /// </summary>
        /// <param name="value">The value of the provided integer.</param>
        public IntMethodArgumentElement(int value)
        {
            Value = value;
        }

        /// <summary>
        /// Gets the integer value.
        /// </summary>
        public int Value { get; }
    }
}
