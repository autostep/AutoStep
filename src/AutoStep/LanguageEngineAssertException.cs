using System;

namespace AutoStep
{
    /// <summary>
    /// Represents an internal language excepion, when the compilation system hits a point that shouldn't be possible due to the parser specification.
    /// </summary>
    public class LanguageEngineAssertException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LanguageEngineAssertException"/> class.
        /// </summary>
        public LanguageEngineAssertException()
            : this(LanguageEngineExceptionMessages.AntlrRulesDoNotMatchExpectation)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LanguageEngineAssertException"/> class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        public LanguageEngineAssertException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LanguageEngineAssertException"/> class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="innerException">The inner exception.</param>
        public LanguageEngineAssertException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
