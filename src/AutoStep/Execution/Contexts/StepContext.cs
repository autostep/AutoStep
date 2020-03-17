using System;
using AutoStep.Elements.Metadata;

namespace AutoStep.Execution.Contexts
{
    /// <summary>
    /// Defines the context type for a running step.
    /// </summary>
    public class StepContext : TestExecutionContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StepContext"/> class.
        /// </summary>
        /// <param name="stepIndex">The step position in the collection.</param>
        /// <param name="parentContext">The parent collection context.</param>
        /// <param name="step">The step reference metadata.</param>
        /// <param name="variables">The variables currently in scope.</param>
        public StepContext(int stepIndex, StepCollectionContext? parentContext, IStepReferenceInfo step, VariableSet variables)
        {
            StepIndex = stepIndex;
            ParentContext = parentContext;
            Step = step;
            Variables = variables;
        }

        /// <summary>
        /// Gets the index of the step in the overall collection.
        /// </summary>
        public int StepIndex { get; }

        /// <summary>
        /// Gets the parent context, the collection of steps.
        /// </summary>
        public StepCollectionContext? ParentContext { get; }

        /// <summary>
        /// Gets the step metadata.
        /// </summary>
        public IStepReferenceInfo Step { get; }

        /// <summary>
        /// Gets the set of variables currently in scope.
        /// </summary>
        public VariableSet Variables { get; }

        /// <summary>
        /// Gets or sets an exception raised while executing the step.
        /// </summary>
        public Exception? FailException { get; set; }

        /// <summary>
        /// Gets or sets the amount of time spent executing the step.
        /// </summary>
        public TimeSpan Elapsed { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the step was actually executed.
        /// If false, an event handler may have decided to skip the step.
        /// </summary>
        public bool StepExecuted { get; set; }
    }
}
