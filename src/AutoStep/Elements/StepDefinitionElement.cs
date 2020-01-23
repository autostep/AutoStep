using System;
using System.Collections.Generic;
using System.Linq;
using AutoStep.Compiler.Matching;
using AutoStep.Elements.Parts;
using AutoStep.Elements.ReadOnly;

namespace AutoStep.Elements
{
    /// <summary>
    /// Represents a built 'Scenario', that can have a name, annotations, a description and a set of steps.
    /// </summary>
    public class StepDefinitionElement : StepCollectionElement, IAnnotatableElement, IStepDefinitionInfo
    {
        private List<DefinitionPart> parts = new List<DefinitionPart>();
        private List<ArgumentPart> arguments = new List<ArgumentPart>();

        /// <summary>
        /// Gets the annotations applied to the step definition, in applied order.
        /// </summary>
        public List<AnnotationElement> Annotations { get; } = new List<AnnotationElement>();

        IReadOnlyList<IAnnotationInfo> IStepDefinitionInfo.Annotations => Annotations;

        /// <summary>
        /// Gets or sets the type of step.
        /// </summary>
        public StepType Type { get; set; }

        /// <summary>
        /// Gets or sets the raw text of the Step declaration.
        /// </summary>
        public string? Declaration { get; set; }

        string IStepDefinitionInfo.Declaration => Declaration ?? throw new LanguageEngineAssertException();

        /// <summary>
        /// Gets or sets the step definition description.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets the set of arguments presented by the Step Definition as being available.
        /// </summary>
        internal IReadOnlyList<ArgumentPart> Arguments => arguments;

        /// <summary>
        /// Gets the set of matching parts used by the step definition element.
        /// </summary>
        internal IReadOnlyList<DefinitionPart> Parts => parts;

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

            return Arguments.Any(a => a.Name == argumentName);
        }

        /// <summary>
        /// Adds a part to the step definition.
        /// </summary>
        /// <param name="part">The part to add.</param>
        internal void AddPart(DefinitionPart part)
        {
            if (part is null)
            {
                throw new ArgumentNullException(nameof(part));
            }

            parts.Add(part);

            if (part is ArgumentPart argPart)
            {
                arguments.Add(argPart);
            }
        }
    }
}
