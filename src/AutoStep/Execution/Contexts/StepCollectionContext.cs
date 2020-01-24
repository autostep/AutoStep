using System;
using AutoStep.Elements.Metadata;

namespace AutoStep.Execution.Contexts
{
    /// <summary>
    /// Defines the context type for a collection of steps.
    /// </summary>
    public class StepCollectionContext : TestExecutionContext
    {
        /// <summary>
        /// Gets or sets the exception that indicates failure of the entire collection.
        /// </summary>
        public Exception? FailException { get; set; }

        /// <summary>
        /// Gets or sets the metadata for the step reference that caused a failure.
        /// </summary>
        public IStepReferenceInfo? FailingStep { get; set; }

        /// <summary>
        /// Gets the elapsed time spent executing the step collection.
        /// </summary>
        public TimeSpan Elapsed { get; internal set; }
    }
}
