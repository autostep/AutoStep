using System;
using System.Collections.Generic;
using System.Linq;
using AutoStep.Core.Matching;

namespace AutoStep.Core.Elements
{
    /// <summary>
    /// Represents a built 'Scenario', that can have a name, annotations, a description and a set of steps.
    /// </summary>
    public class StepDefinitionElement : StepCollectionElement, IAnnotatableElement
    {
        private List<StepMatchingPart> matchingParts = new List<StepMatchingPart>();

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

        internal IReadOnlyList<StepMatchingPart> MatchingParts => matchingParts;

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

        public void UpdateFromStepReference(StepReferenceElement step)
        {
            if (step is null)
            {
                throw new ArgumentNullException(nameof(step));
            }

            if (step.Arguments != null)
            {
                if (Arguments == null)
                {
                    Arguments = new List<StepArgumentElement>();
                }

                Arguments.AddRange(step.Arguments);
            }

            Type = step.Type;
            Declaration = step.RawText;
            matchingParts.AddRange(step.MatchingParts);
        }

        public void AddMatchingPart(string text)
        {
            matchingParts.Add(new StepMatchingPart(text));
        }

        public void AddMatchingPart(ArgumentType argType)
        {
            matchingParts.Add(new StepMatchingPart(argType));
        }
    }
}
