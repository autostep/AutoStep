using System.Collections.Generic;

namespace AutoStep.Core
{
    /// <summary>
    /// Defines an element that can contain a list of steps.
    /// </summary>
    public abstract class BuiltStepCollection : BuiltElement
    {
        /// <summary>
        /// Gets the set of steps in the collection.
        /// </summary>
        public List<StepReference> Steps { get; } = new List<StepReference>();
    }
}
