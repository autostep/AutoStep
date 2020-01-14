using System;
using System.Collections.Generic;
using AutoStep.Compiler.Matching;
using AutoStep.Definitions;
using AutoStep.Elements.Parts;

namespace AutoStep.Elements
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
        private List<ContentPart> workingParts = new List<ContentPart>();
        private ContentPart[] frozenParts = null;

        /// <summary>
        /// Gets or sets the determined <see cref="StepType"/> used to bind against a declared Step. This will usually only differ
        /// from <see cref="StepReferenceElement.Type"/> when the step is of the <see cref="StepType.And"/> type, and the binding is determined by a preceding step.
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
        public string? RawText { get; set; }

        /// <summary>
        /// Gets the bound step definition; will be null if the step cannot be bound, or the linker could not find a matching step definition.
        /// </summary>
        public StepDefinition? BoundDefinition { get; private set; }

        /// <summary>
        /// Gets the generated 'matching parts' used by the linker to associate step references to definitions.
        /// </summary>
        public ReadOnlySpan<ContentPart> PartSpan => frozenParts ?? throw new InvalidOperationException("Parts have not been frozen.");

        public IEnumerable<ContentPart> Parts => frozenParts ?? (IEnumerable<ContentPart>)workingParts;

        /// <summary>
        /// Gets or sets the associated table for this step.
        /// </summary>
        public TableElement? Table { get; set; }

        /// <summary>
        /// Adds a part to the step reference.
        /// </summary>
        /// <param name="part">The part to add.</param>
        public void AddPart(ContentPart part)
        {
            if (part is null)
            {
                throw new System.ArgumentNullException(nameof(part));
            }

            if (frozenParts is object)
            {
                throw new InvalidOperationException("Step parts have been frozen.");
            }

            workingParts.Add(part);
        }

        public void FreezeParts()
        {
            frozenParts = workingParts.ToArray();

            // Wipe the working parts, we aren't going to be using them anymore.
            workingParts = null!;
        }

        /// <summary>
        /// Bind a given step definition onto the step reference.
        /// </summary>
        /// <param name="definition">The definition that provides this step reference.</param>
        public void Bind(StepDefinition definition)
        {
            BoundDefinition = definition ?? throw new ArgumentNullException(nameof(definition));
        }

        /// <summary>
        /// Unbinds a step reference from an existing definition (perhaps because the definition is no longer available).
        /// </summary>
        public void Unbind()
        {
            BoundDefinition = null;
        }
    }
}
