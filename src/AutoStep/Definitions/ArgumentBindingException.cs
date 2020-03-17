using System;

namespace AutoStep.Definitions
{
    /// <summary>
    /// Represents an exception thrown while binding a text argument to a typed value.
    /// </summary>
    public class ArgumentBindingException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ArgumentBindingException"/> class.
        /// </summary>
        /// <param name="textValue">The textual value that caused a problem.</param>
        /// <param name="expectedType">The type we were expecting to get from the binding.</param>
        /// <param name="innerException">The error thrown by the binder.</param>
        public ArgumentBindingException(string textValue, Type expectedType, Exception innerException)
            : base(FormatMessage(textValue, expectedType), innerException)
        {
            TextValue = textValue;
            ExpectedType = expectedType;
        }

        /// <summary>
        /// Gets the text value that failed binding.
        /// </summary>
        public string TextValue { get; }

        /// <summary>
        /// Gets the type we tried to bind to.
        /// </summary>
        public Type ExpectedType { get; }

        private static string FormatMessage(string textValue, Type expectedType)
        {
            if (expectedType is null)
            {
                throw new ArgumentNullException(nameof(expectedType));
            }

            return $"Failed to bind input value '{textValue}' to an instance of '{expectedType.Name}'";
        }
    }
}
