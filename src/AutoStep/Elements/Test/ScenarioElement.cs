using System.Collections.Generic;
using AutoStep.Elements.Metadata;

namespace AutoStep.Elements.Test
{
    /// <summary>
    /// Represents a built 'Scenario', that can have a name, annotations, a description and a set of steps.
    /// </summary>
    public class ScenarioElement : StepCollectionElement, IAnnotatableElement, IScenarioInfo
    {
        /// <summary>
        /// Gets the annotations applied to the feature, in applied order.
        /// </summary>
        public List<AnnotationElement> Annotations { get; } = new List<AnnotationElement>();

        /// <inheritdoc/>
        IReadOnlyList<IAnnotationInfo> IScenarioInfo.Annotations => Annotations;

        /// <summary>
        /// Gets or sets the name of the scenario.
        /// </summary>
        public string? Name { get; set; }

        /// <inheritdoc/>
        string IScenarioInfo.Name => Name ?? throw new LanguageEngineAssertException();

        /// <summary>
        /// Gets or sets the scenario description.
        /// </summary>
        public string? Description { get; set; }
    }
}
