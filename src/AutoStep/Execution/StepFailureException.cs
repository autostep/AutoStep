using System;
using System.Collections.Generic;
using System.Text;
using AutoStep.Elements;
using AutoStep.Elements.Metadata;

namespace AutoStep.Execution
{
    /// <summary>
    /// Represents an error that occurred during a step (for example, an assert failure).
    /// </summary>
    public class StepFailureException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StepFailureException"/> class.
        /// </summary>
        /// <param name="stepReference">The step metadata.</param>
        /// <param name="innerException">The underlying exception.</param>
        public StepFailureException(IStepReferenceInfo stepReference, Exception innerException)
            : base(ExecutionText.StepFailureException_Message, innerException)
        {
            Step = stepReference;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StepFailureException"/> class.
        /// </summary>
        /// <param name="stepReference">The step metadata.</param>
        /// <param name="message">The exception message.</param>
        public StepFailureException(IStepReferenceInfo stepReference, string message)
            : base(message)
        {
            Step = stepReference;
        }

        /// <summary>
        /// Gets the step metadata for the step that raised the error.
        /// </summary>
        public IStepReferenceInfo Step { get; }
    }
}
