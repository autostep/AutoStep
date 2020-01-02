namespace AutoStep.Core.Elements
{
    /// <summary>
    /// Defines an option annotation.
    /// </summary>
    public class OptionElement : AnnotationElement
    {
        /// <summary>
        /// Gets or sets the option name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the setting name.
        /// </summary>
        public string Setting { get; set; }
    }
}
