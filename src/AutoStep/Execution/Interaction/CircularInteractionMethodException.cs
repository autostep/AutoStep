using System;
using System.Collections.Generic;
using AutoStep.Definitions;
using AutoStep.Elements.Interaction;
using AutoStep.Elements.Metadata;

namespace AutoStep.Execution.Interaction
{
    /// <summary>
    /// Represents an exception thrown when a circular interaction method loop is detected.
    /// </summary>
    public class CircularInteractionMethodException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CircularInteractionMethodException"/> class.
        /// </summary>
        /// <param name="methodCall">The method call that initiates the loop.</param>
        /// <param name="methodContextStack">The method execution stack at the time the loop was detected.</param>
        public CircularInteractionMethodException(MethodCallElement methodCall, IEnumerable<MethodContext> methodContextStack)
            : base(ExecutionText.CircularInteractionMethodException_Message)
        {
            MethodCall = methodCall;
            MethodContextStack = methodContextStack;
        }

        /// <summary>
        /// Gets the method call that initiates the loop.
        /// </summary>
        public MethodCallElement MethodCall { get; }

        /// <summary>
        /// Gets the method execution stack at the time the loop was detected.
        /// </summary>
        public IEnumerable<MethodContext> MethodContextStack { get; }
    }
}
