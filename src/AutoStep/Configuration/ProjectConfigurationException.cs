using System;

namespace AutoStep
{
    /// <summary>
    /// An exception thrown because of an issue with the loaded project configuration.
    /// </summary>
    public class ProjectConfigurationException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectConfigurationException"/> class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        public ProjectConfigurationException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectConfigurationException"/> class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="innerException">The underlying exception.</param>
        public ProjectConfigurationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
