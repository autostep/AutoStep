using System.Collections.Generic;

namespace AutoStep.Core
{
    /// <summary>
    /// Represents a reference to a Step inside a written test.
    /// </summary>
    /// <remarks>
    /// The base <see cref="StepReference"/> class
    /// does not understand binding or what can run, it just defines the information about the written line in the test.
    /// </remarks>
    public class StepReference : BuiltElement
    {
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
        public List<StepArgument> Arguments { get; private set; }

        /// <summary>
        /// Adds an argument to the step reference.
        /// </summary>
        /// <param name="argument">The argument to add.</param>
        public void AddArgument(StepArgument argument)
        {
            if (Arguments == null)
            {
                Arguments = new List<StepArgument>();
            }

            Arguments.Add(argument);
        }
    }
}
