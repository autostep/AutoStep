using AutoStep.Elements.Metadata;

namespace AutoStep.Elements
{
    /// <summary>
    /// Defines an option annotation.
    /// </summary>
    public class OptionElement : AnnotationElement, IOptionInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OptionElement"/> class.
        /// </summary>
        /// <param name="name">The option name.</param>
        public OptionElement(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Gets or sets the option name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the setting name.
        /// </summary>
        public string? Setting { get; set; }
    }
}
