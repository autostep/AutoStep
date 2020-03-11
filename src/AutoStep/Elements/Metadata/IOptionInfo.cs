namespace AutoStep.Elements.Metadata
{
    /// <summary>
    /// Metadata for an Option Annotation ($).
    /// </summary>
    public interface IOptionInfo : IAnnotationInfo
    {
        /// <summary>
        /// Gets the name of the option.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the setting value for the option.
        /// </summary>
        string? Setting { get; }
    }
}
