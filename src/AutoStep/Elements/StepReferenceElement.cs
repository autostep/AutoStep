using System;
using System.Collections.Generic;
using AutoStep.Language;
using AutoStep.Elements.Metadata;
using AutoStep.Elements.StepTokens;

namespace AutoStep.Elements
{
    /// <summary>
    /// Represents a reference to a Step inside a written test.
    /// </summary>
    /// <remarks>
    /// The base <see cref="StepReferenceElement"/> class
    /// does not understand binding or what can run, it just defines the information about the written line in the test.
    /// </remarks>
    public class StepReferenceElement : BuiltElement, IStepReferenceInfo
    {
        private List<StepToken> workingTokens = new List<StepToken>();
        private StepToken[]? frozenTokens = null;

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

        /// <inheritdoc/>
        string IStepReferenceInfo.Text => RawText ?? throw new LanguageEngineAssertException();

        /// <summary>
        /// Gets the bound step definition; will be null if the step cannot be bound, or the linker could not find a matching step definition.
        /// </summary>
        public StepReferenceBinding? Binding { get; private set; }

        /// <summary>
        /// Gets the generated 'matching parts' used by the linker to associate step references to definitions.
        /// </summary>
        internal ReadOnlySpan<StepToken> TokenSpan => frozenTokens ?? throw new InvalidOperationException(ElementExceptionMessages.TokensNotFrozen);

        /// <inheritdoc/>
        ReadOnlySpan<StepToken> IStepReferenceInfo.TokenSpan => TokenSpan;

        /// <summary>
        /// Gets or sets the associated table for this step.
        /// </summary>
        public TableElement? Table { get; set; }

        /// <inheritdoc/>
        ITableInfo? IStepReferenceInfo.Table => Table;

        /// <summary>
        /// Adds a token to the step reference.
        /// </summary>
        /// <param name="token">The part to add.</param>
        internal void AddToken(StepToken token)
        {
            if (token is null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            if (frozenTokens is object)
            {
                throw new InvalidOperationException(ElementExceptionMessages.TokensAlreadyFrozen);
            }

            workingTokens.Add(token);
        }

        /// <summary>
        /// Freezes the set of working tokens into an array of tokens (so that a span can be constructed).
        /// </summary>
        internal void FreezeTokens()
        {
            if (frozenTokens is object)
            {
                throw new InvalidOperationException(ElementExceptionMessages.TokensAlreadyFrozen);
            }

            frozenTokens = workingTokens.ToArray();

            // Wipe the working parts, we aren't going to be using them anymore.
            workingTokens = null!;
        }

        /// <summary>
        /// Bind a given step definition onto the step reference.
        /// </summary>
        /// <param name="binding">The binding that provides this step reference.</param>
        public void Bind(StepReferenceBinding binding)
        {
            Binding = binding ?? throw new ArgumentNullException(nameof(binding));
        }

        /// <summary>
        /// Unbinds a step reference from an existing definition (perhaps because the definition is no longer available).
        /// </summary>
        public void Unbind()
        {
            Binding = null;
        }
    }
}
