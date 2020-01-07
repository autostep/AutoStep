namespace AutoStep.Elements
{
    /// <summary>
    /// Defines a single part of a statement argument.
    /// </summary>
    public class ArgumentSectionElement : PositionalElement
    {
        /// <summary>
        /// Gets or sets the raw text of the section.
        /// </summary>
        public string? RawText { get; set; }

        /// <summary>
        /// Gets or sets the escaped text for the section.
        /// </summary>
        public string? EscapedText { get; set; }

        /// <summary>
        /// Gets or sets the name of the example variable (if one is specified).
        /// </summary>
        public string? ExampleInsertionName { get; set; }
    }
}
