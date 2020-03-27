using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AutoStep.Definitions;
using AutoStep.Elements.Parts;

namespace AutoStep.Language.Test.Matching
{
    /// <summary>
    /// Defines a match result found while searching the MatchingTree.
    /// </summary>
    internal class MatchResult : IMatchResult
    {
        private List<LanguageOperationMessage>? msgs;
        private Dictionary<string, string>? placeholderValues;

        /// <summary>
        /// Initializes a new instance of the <see cref="MatchResult"/> class.
        /// </summary>
        /// <param name="isExact">Whether this is an exact match.</param>
        /// <param name="confidence">The confidence level.</param>
        /// <param name="definition">The matched definition.</param>
        public MatchResult(bool isExact, int confidence, StepDefinition definition)
        {
            IsExact = isExact;
            Confidence = confidence;
            Definition = definition;
            msgs = null;
            ArgumentSet = null;
        }

        /// <summary>
        /// Gets the confidence rating for the match. A higher confidence indicates that the match
        /// is more likely to be right for the provided input.
        /// </summary>
        public int Confidence { get; }

        /// <summary>
        /// Gets the matched definition.
        /// </summary>
        public StepDefinition Definition { get; }

        /// <summary>
        /// Gets a value indicating whether this is an exact one for the definition.
        /// </summary>
        public bool IsExact { get; }

        /// <summary>
        /// Gets the set of argument bindings for this result.
        /// </summary>
        public LinkedList<ArgumentBinding>? ArgumentSet { get; private set; }

        /// <inheritdoc/>
        public IEnumerable<ArgumentBinding> Arguments => ArgumentSet ?? Enumerable.Empty<ArgumentBinding>();

        /// <summary>
        /// Gets the set of matched placeholder values for the match result.
        /// </summary>
        public IReadOnlyDictionary<string, string>? PlaceholderValues => placeholderValues;

        /// <summary>
        /// Adds a compiler message to the match result.
        /// </summary>
        /// <param name="msg">The compilation message.</param>
        public void AddMessage(LanguageOperationMessage msg)
        {
            if (msgs is null)
            {
                msgs = new List<LanguageOperationMessage>();
            }

            msgs.Add(msg);
        }

        /// <summary>
        /// Prepends (inserts at the beginning) an argument set to the overall match result, using an individual part match.
        /// </summary>
        /// <param name="arg">The argument part.</param>
        /// <param name="matchResult">The result of the argument matching.</param>
        public void PrependArgumentSet(ArgumentPart arg, StepReferenceMatchResult matchResult)
        {
            if (ArgumentSet is null)
            {
                ArgumentSet = new LinkedList<ArgumentBinding>();
            }

            ArgumentSet.AddFirst(new ArgumentBinding(arg, matchResult));
        }

        /// <summary>
        /// Include a named placeholder value in the match result.
        /// </summary>
        /// <param name="name">The name of the placeholder.</param>
        /// <param name="value">The value of the placeholder.</param>
        public void IncludePlaceholderValue(string name, string value)
        {
            if (placeholderValues is null)
            {
                placeholderValues = new Dictionary<string, string>();
            }

            placeholderValues[name] = value;
        }

        /// <summary>
        /// Attempt to retrieve a named placeholder value from the match result.
        /// </summary>
        /// <param name="name">The name of the placeholder.</param>
        /// <param name="value">The value of the placeholder.</param>
        /// <returns>True if the value was found; false otherwise.</returns>
        public bool TryGetPlaceholderValue(string name, [NotNullWhen(true)] out string? value)
        {
            if (placeholderValues is null)
            {
                value = null;
                return false;
            }

            return placeholderValues.TryGetValue(name, out value);
        }
    }
}
