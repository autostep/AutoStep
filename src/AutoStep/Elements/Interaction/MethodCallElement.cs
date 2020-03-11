using System.Collections.Generic;

namespace AutoStep.Elements.Interaction
{
    /// <summary>
    /// Represents an interaction method call.
    /// </summary>
    public class MethodCallElement : PositionalElement
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MethodCallElement"/> class.
        /// </summary>
        /// <param name="methodName">The name of the method.</param>
        public MethodCallElement(string methodName)
        {
            MethodName = methodName;
        }

        /// <summary>
        /// Gets the name of the method.
        /// </summary>
        public string MethodName { get; }

        /// <summary>
        /// Gets the set of arguments to the method.
        /// </summary>
        public List<MethodArgumentElement> Arguments { get; } = new List<MethodArgumentElement>();
    }
}
