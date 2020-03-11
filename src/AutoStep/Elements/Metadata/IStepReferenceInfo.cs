using System;
using System.Text;
using AutoStep.Elements.StepTokens;
using AutoStep.Language.Test;

namespace AutoStep.Elements.Metadata
{
    /// <summary>
    /// Metadata for an executing Step Reference.
    /// </summary>
    public interface IStepReferenceInfo : IElementInfo
    {
        /// <summary>
        /// Gets the binding type of the step (determines which definition it is bound to).
        /// </summary>
        StepType? BindingType { get; }

        /// <summary>
        /// Gets the declared type of the step (including And).
        /// </summary>
        StepType Type { get; }

        /// <summary>
        /// Gets the text body of the step.
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// Gets an attached table, if there is one.
        /// </summary>
        public ITableInfo? Table { get; }

        /// <summary>
        /// Gets the linked step binding.
        /// </summary>
        public StepReferenceBinding? Binding { get; }

        /// <summary>
        /// Gets the span of tokens for this step reference. Internal.
        /// </summary>
        internal ReadOnlySpan<StepToken> TokenSpan { get; }
    }
}
