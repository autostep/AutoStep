using System;

namespace AutoStep.Execution.Binding
{
    /// <summary>
    /// Defines an interface for binding a textual value to a destination type.
    /// </summary>
    public interface IArgumentBinder
    {
        /// <summary>
        /// Retrieve an expected destination type value from a text value.
        /// </summary>
        /// <param name="textValue">The text to bind.</param>
        /// <param name="destinationType">The destination type to convert to.</param>
        /// <returns>The bound value (if the value can be bound).</returns>
        object Bind(string textValue, Type destinationType);
    }
}
