using System;

namespace AutoStep.Assertion
{
    /// <summary>
    /// Represents a test assertion failure.
    /// </summary>
    public class AssertionException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AssertionException"/> class.
        /// </summary>
        /// <param name="message">The assertion message.</param>
        public AssertionException(string message)
            : base(message)
        {
        }
    }
}
