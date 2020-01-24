using System;

namespace AutoStep.Execution.Dependency
{
    /// <summary>
    /// Represents an error in the DI system. This is often just a wrapper for the underlying container system.
    /// </summary>
    public class DependencyException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DependencyException"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        public DependencyException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DependencyException"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The underlying exception.</param>
        public DependencyException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
