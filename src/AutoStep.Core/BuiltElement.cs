using System.Collections.Generic;

namespace AutoStep.Core
{
    /// <summary>
    /// Represents a generic built element that has a position in a file.
    /// </summary>
    public class BuiltElement
    {
        /// <summary>
        /// Gets or sets the line number in the source.
        /// </summary>
        public int SourceLine { get; set; }

        /// <summary>
        /// Gets or sets the column position on the line.
        /// </summary>
        public int SourceColumn { get; set; }
    }
}
