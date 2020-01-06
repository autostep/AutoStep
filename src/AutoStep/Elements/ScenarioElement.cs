using System.Collections.Generic;

namespace AutoStep.Elements
{
    /// <summary>
    /// Represents a built 'Scenario', that can have a name, annotations, a description and a set of steps.
    /// </summary>
    public class ScenarioElement : StepCollectionElement, IAnnotatableElement
    {
        /// <summary>
        /// Gets the annotations applied to the feature, in applied order.
        /// </summary>
        public List<AnnotationElement> Annotations { get; } = new List<AnnotationElement>();

        /// <summary>
        /// Gets or sets the name of the scenario.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the scenario description.
        /// </summary>
        public string? Description { get; set; }
    }
}
