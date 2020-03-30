using System.Collections.Generic;
using AutoStep.Elements.Metadata;

namespace AutoStep.Elements.Test
{
    /// <summary>
    /// Defines an element that can contain a list of steps.
    /// </summary>
    public abstract class StepCollectionElement : PositionalElement, IStepCollectionInfo
    {
        /// <summary>
        /// Gets the set of steps in the collection.
        /// </summary>
        public List<StepReferenceElement> Steps { get; private set; } = new List<StepReferenceElement>();

        /// <inheritdoc/>
        IReadOnlyList<IStepReferenceInfo> IStepCollectionInfo.Steps => Steps;

        /// <summary>
        /// Instructs this element to directly reference the step collection from another element.
        /// </summary>
        /// <param name="other">The step collection whose steps should be used.</param>
        public void UseStepsFrom(StepCollectionElement other)
        {
            if (other is null)
            {
                throw new System.ArgumentNullException(nameof(other));
            }

            // Note that this is a direct reference rather than a
            // clone.
            Steps = other.Steps;
        }
    }
}
