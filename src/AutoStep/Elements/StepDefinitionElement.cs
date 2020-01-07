using System;
using System.Collections.Generic;
using System.Linq;
using AutoStep.Compiler.Matching;

namespace AutoStep.Elements
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
        /// Gets or sets the type of step.
        /// </summary>
        public StepType Type { get; set; }

        /// <summary>
        /// Gets or sets the raw text of the Step declaration.
        /// </summary>
        public string? Declaration { get; set; }

        /// <summary>
        /// Gets the set of arguments presented by the Step Definition as being available.
        /// </summary>
        public List<StepArgumentElement>? Arguments { get; private set; }

        /// <summary>
        /// Gets or sets the scenario description.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets the set of matching parts used by the step definition element.
        /// </summary>
        internal IReadOnlyList<StepMatchingPart> MatchingParts => matchingParts;

        /// <summary>
        /// Check if this step definition contains an argument with the specified name.
        /// </summary>
        /// <param name="argumentName">The argument to check for.</param>
        /// <returns>true if available, false otherwise.</returns>
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
                throw new ArgumentNullException(nameof(argument));
            }

            if (Arguments == null)
            {
                Arguments = new List<StepArgumentElement>();
            }

            Arguments.Add(argument);
        }

        /// <summary>
        /// Update this Step Definition from a parsed step reference.
        /// </summary>
        /// <param name="step">The step reference.</param>
        /// <remarks>
        /// The compiler treats step definition statements as step references until we convert them,
        /// just so we can re-use the same compiler tree visitors.
        /// </remarks>
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

        /// <summary>
        /// Add a definition text matching part.
        /// </summary>
        /// <param name="text">The text content.</param>
        public void AddMatchingPart(string text)
        {
            matchingParts.Add(new StepMatchingPart(text));
        }

        /// <summary>
        /// Add a definition argument matching part.
        /// </summary>
        /// <param name="argType">The type of the argument.</param>
        public void AddMatchingPart(ArgumentType argType)
        {
            matchingParts.Add(new StepMatchingPart(argType));
        }
    }
}
