using System.Collections.Generic;
using System.Linq;

namespace AutoStep.Core.Elements
{
    /// <summary>
    /// Represents a built 'Scenario', that can have a name, annotations, a description and a set of steps.
    /// </summary>
    public class StepDefinitionElement : StepCollectionElement, IAnnotatableElement
    {
        /// <summary>
        /// Gets the annotations applied to the feature, in applied order.
        /// </summary>
        public List<AnnotationElement> Annotations { get; } = new List<AnnotationElement>();

        /// <summary>
        /// Gets or sets the name of the scenario.
        /// </summary>
        public StepType Type { get; set; }

        /// <summary>
        /// Gets or sets the raw text of the Step declaration.
        /// </summary>
        public string Declaration { get; set; }

        /// <summary>
        /// Gets the set of arguments presented by the Step Definition as being available.
        /// </summary>
        public List<StepArgumentElement> Arguments { get; private set; }

        /// <summary>
        /// Gets or sets the scenario description.
        /// </summary>
        public string Description { get; set; }

        public bool ContainsArgument(string argumentName)
        {
            if (Arguments == null)
            {
                return false;
            }

            return Arguments.Any(a => a.RawArgument == argumentName);
        }

        /// <summary>
        /// Adds an argument to the step definition.
        /// </summary>
        /// <param name="argument">The argument to add.</param>
        public void AddArgument(StepArgumentElement argument)
        {
            if (argument is null)
            {
                throw new System.ArgumentNullException(nameof(argument));
            }

            if (Arguments == null)
            {
                Arguments = new List<StepArgumentElement>();
            }

            Arguments.Add(argument);
        }

        /// <summary>
        /// Adds multiple arguments to the step definition.
        /// </summary>
        /// <param name="arguments">The arguments to add.</param>
        public void AddArguments(IEnumerable<StepArgumentElement> arguments)
        {
            if (arguments is null)
            {
                throw new System.ArgumentNullException(nameof(arguments));
            }

            if (Arguments == null)
            {
                Arguments = new List<StepArgumentElement>();
            }

            Arguments.AddRange(arguments);
        }
    }
}
