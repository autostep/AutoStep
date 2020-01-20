using System.Collections.Generic;
using AutoStep.Elements.ReadOnly;

namespace AutoStep.Elements
{
    /// <summary>
    /// Defines an element that can contain a list of steps.
    /// </summary>
    public abstract class StepCollectionElement : BuiltElement, IStepCollectionInfo
    {
        /// <summary>
        /// Gets the set of steps in the collection.
        /// </summary>
        public List<StepReferenceElement> Steps { get; private set; } = new List<StepReferenceElement>();

        IReadOnlyList<IStepReferenceInfo> IStepCollectionInfo.Steps => Steps;

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
