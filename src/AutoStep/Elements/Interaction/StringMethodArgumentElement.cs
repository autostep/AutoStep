using AutoStep.Language;

namespace AutoStep.Elements.Interaction
{
    /// <summary>
    /// Represents a string argument to a method.
    /// </summary>
    public class StringMethodArgumentElement : MethodArgumentElement
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StringMethodArgumentElement"/> class.
        /// </summary>
        /// <param name="text">The string text.</param>
        public StringMethodArgumentElement(string text)
        {
            Text = text;
        }

        /// <summary>
        /// Gets the literal text.
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// Gets or sets the tokenised data for the text to allow variable replacement.
        /// </summary>
        public TokenisedArgumentValue? Tokenised { get; set; }
    }
}
