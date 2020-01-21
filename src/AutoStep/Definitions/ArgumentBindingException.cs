using System;
using System.Runtime.Serialization;

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
        public ArgumentBindingException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArgumentBindingException"/> class.
        /// </summary>
        /// <param name="textValue">The textual value that caused a problem.</param>
        /// <param name="expectedType">The type we were expecting to get from the binding.</param>
        /// <param name="innerException">The error thrown by the binder.</param>
        public ArgumentBindingException(string textValue, Type expectedType, Exception innerException)
            : this(FormatMessage(textValue, expectedType), innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArgumentBindingException"/> class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        public ArgumentBindingException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArgumentBindingException"/> class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="innerException">The underlying exception.</param>
        public ArgumentBindingException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

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
