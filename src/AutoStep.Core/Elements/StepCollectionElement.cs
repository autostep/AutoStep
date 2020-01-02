using System.Collections.Generic;

namespace AutoStep.Core.Elements
{
    /// <summary>
    /// Defines an element that can contain a list of steps.
    /// </summary>
    public abstract class StepCollectionElement : BuiltElement
    {
        /// <summary>
        /// Gets the set of steps in the collection.
        /// </summary>
        public List<StepReferenceElement> Steps { get; } = new List<StepReferenceElement>();
    }
}
