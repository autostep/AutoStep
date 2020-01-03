using System.Collections.Generic;

namespace AutoStep.Core.Elements
{

    /// <summary>
    /// Represents a reference to a Step inside a written test.
    /// </summary>
    /// <remarks>
    /// The base <see cref="StepReferenceElement"/> class
    /// does not understand binding or what can run, it just defines the information about the written line in the test.
    /// </remarks>
    public class StepReferenceElement : BuiltElement
    {
        private List<StepArgumentElement> arguments;
        private List<StepMatchingPart> matchingParts = new List<StepMatchingPart>();

        /// <summary>
        /// Gets or sets the determined <see cref="StepType"/> used to bind against a declared Step. This will usually only differ
        /// from <see cref="Type"/> when the step is of the <see cref="StepType.And"/> type, and the binding is determined by a preceding step.
        /// A null value indicates there was no preceding step, so a compilation error probably occurred anyway.
        /// </summary>
        public StepType? BindingType { get; set; }

        /// <summary>
        /// Gets or sets the declared type of the step reference.
        /// </summary>
        public StepType Type { get; set; }

        /// <summary>
        /// Gets or sets the raw text of the Step Reference, without having processed any escaped characters.
        /// </summary>
        public string RawText { get; set; }

        /// <summary>
        /// Gets the set of arguments presented by the Step Reference.
        /// </summary>
        public IReadOnlyCollection<StepArgumentElement> Arguments => arguments;

        internal IReadOnlyList<StepMatchingPart> MatchingParts => matchingParts;

        /// <summary>
        /// Gets or sets the associated table for this step.
        /// </summary>
        public TableElement Table { get; set; }

        /// <summary>
        /// Adds an argument to the step reference.
        /// </summary>
        /// <param name="argument">The argument to add.</param>
        public void AddArgument(StepArgumentElement argument)
        {
            if (argument is null)
            {
                throw new System.ArgumentNullException(nameof(argument));
            }

            if (arguments == null)
            {
                arguments = new List<StepArgumentElement>();
            }

            matchingParts.Add(new StepMatchingPart(argument.Type));

            arguments.Add(argument);
        }

        public void AddMatchingText(string textContent)
        {
            if (textContent is null)
            {
                throw new System.ArgumentNullException(nameof(textContent));
            }

            matchingParts.Add(new StepMatchingPart(textContent));
        }
    }
}
