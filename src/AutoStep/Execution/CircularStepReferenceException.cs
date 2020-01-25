using System;
using System.Collections.Generic;
using AutoStep.Definitions;
using AutoStep.Elements.Metadata;

namespace AutoStep.Execution
{
    /// <summary>
    /// Represents an exception thrown when a circular step loop is detected.
    /// </summary>
    public class CircularStepReferenceException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CircularStepReferenceException"/> class.
        /// </summary>
        /// <param name="stepDefinition">The step definition that initiates the loop.</param>
        /// <param name="stepExecutionStack">The stack of step invocations that lead to the error.</param>
        public CircularStepReferenceException(StepDefinition stepDefinition, IEnumerable<IStepReferenceInfo> stepExecutionStack)
            : base(ExecutionText.CircularStepReferenceException_Message)
        {
            StepDefinition = stepDefinition;
            StepExecutionStack = stepExecutionStack;
        }

        /// <summary>
        /// Gets the step definition that initiates the loop.
        /// </summary>
        public StepDefinition StepDefinition { get; }

        /// <summary>
        /// Gets the step execution stack of all step references involved in the detected loop.
        /// </summary>
        public IEnumerable<IStepReferenceInfo> StepExecutionStack { get; }
    }
}
