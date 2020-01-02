namespace AutoStep.Core.Elements
{
    /// <summary>
    /// Represents a built element that understands it ends before line terminates (e.g. an argument).
    /// </summary>
    public class PositionalElement : BuiltElement
    {
        /// <summary>
        /// Gets or sets the column position at which the element ends.
        /// </summary>
        public int EndColumn { get; set; }
    }
}
