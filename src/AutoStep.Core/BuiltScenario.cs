using System.Collections.Generic;

namespace AutoStep.Core
{
    /// <summary>
    /// Represents a built 'Scenario', that can have a name, annotations, a description and a set of steps.
    /// </summary>
    public class BuiltScenario : BuiltStepCollection, IAnnotatable
    {
        /// <summary>
        /// Gets the annotations applied to the feature, in applied order.
        /// </summary>
        public List<AnnotationElement> Annotations { get; } = new List<AnnotationElement>();

        /// <summary>
        /// Gets or sets the name of the scenario.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the scenario description.
        /// </summary>
        public string Description { get; set; }
    }

    public class BuiltExample : BuiltElement, IAnnotatable
    {
        public List<AnnotationElement> Annotations { get; } = new List<AnnotationElement>();

        public BuiltTable Table { get; set; }
    }

    public class BuiltScenarioOutline : BuiltScenario
    {
        public List<BuiltExample> Examples { get; } = new List<BuiltExample>();
    }
}
