using System;
using System.Collections.Generic;

namespace AutoStep.Elements
{
    /// <summary>
    /// Represents a provided argument to a Step Reference.
    /// </summary>
    public class StepArgumentElement : PositionalElement
    {
        private List<ArgumentSectionElement> sections = new List<ArgumentSectionElement>();

        /// <summary>
        /// Gets or sets the raw argument text.
        /// </summary>
        public string? RawArgument { get; set; }

        /// <summary>
        /// Gets or sets the escaped argument body.
        /// </summary>
        public string? EscapedArgument { get; set; }

        /// <summary>
        /// Gets or sets the parsed value of the argument. Null for interpolated arguments and arguments that contain example replacements.
        /// </summary>
        public object? Value { get; set; }

        /// <summary>
        /// Gets or sets the argument type.
        /// </summary>
        public ArgumentType Type { get; set; }

        /// <summary>
        /// Gets or sets an optional leading symbol from the original value ($, £, etc).
        /// </summary>
        public string? Symbol { get; set; }

        /// <summary>
        /// Gets or the parsed sections within the argument.
        /// </summary>
        public IReadOnlyList<ArgumentSectionElement> Sections { get => sections; }

        /// <summary>
        /// Add a section to the argument.
        /// </summary>
        /// <param name="section">The section.</param>
        public void AddSection(ArgumentSectionElement section)
        {
            if (section is null)
            {
                throw new ArgumentNullException(nameof(section));
            }

            sections.Add(section);
        }

        /// <summary>
        /// Adds multiple sections to the argument.
        /// </summary>
        /// <param name="newSections">The new sections.</param>
        public void ReplaceSections(IEnumerable<ArgumentSectionElement> newSections)
        {
            if (newSections is null)
            {
                throw new ArgumentNullException(nameof(newSections));
            }

            sections.Clear();
            sections.AddRange(newSections);
        }
    }
}
