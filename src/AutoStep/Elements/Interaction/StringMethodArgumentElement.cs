using System;
using System.Collections.Generic;
using System.Text;
using AutoStep.Language;

namespace AutoStep.Elements.Interaction
{
    /// <summary>
    /// Represents a string arugment to a method.
    /// </summary>
    public class StringMethodArgumentElement : MethodArgumentElement
    {
        /// <summary>
        /// Gets or sets the literal text.
        /// </summary>
        public string? Text { get; set; }

        /// <summary>
        /// Gets or sets the tokenised data for the text to allow variable replacement.
        /// </summary>
        public TokenisedArgumentValue? Tokenised { get; set; }
    }
}
