using System.Collections.Generic;

namespace AutoStep.Elements.Metadata
{
    /// <summary>
    /// Metadata for a Step Definition declared in an AutoStep file.
    /// </summary>
    public interface IStepDefinitionInfo : IElementInfo
    {
        /// <summary>
        /// Gets the binding type of the step definition.
        /// </summary>
        StepType Type { get; }

        /// <summary>
        /// Gets the declaration text for the step definition.
        /// </summary>
        string Declaration { get; }

        /// <summary>
        /// Gets the optional description for the step definition.
        /// </summary>
        string? Description { get; }

        /// <summary>
        /// Gets the set of annotations applied to the step definition.
        /// </summary>
        IReadOnlyList<IAnnotationInfo> Annotations { get; }
    }
}
