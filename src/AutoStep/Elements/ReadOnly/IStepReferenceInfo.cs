using System;
using System.Text;
using AutoStep.Compiler;
using AutoStep.Elements.StepTokens;

namespace AutoStep.Elements.ReadOnly
{
    public interface IStepReferenceInfo : IElementInfo
    {
        StepType? BindingType { get; }

        StepType Type { get; }

        public string Text { get; }

        public ITableInfo? Table { get; }

        public StepReferenceBinding? Binding { get; }

        internal ReadOnlySpan<StepToken> TokenSpan { get; }
    }
}
