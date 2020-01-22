using System;
using System.Collections.Generic;
using AutoStep.Elements.StepTokens;

namespace AutoStep.Elements.ReadOnly
{
    public interface ITableCellInfo : IPositionalElementInfo
    {
        string? Text { get; }

        internal IReadOnlyList<StepToken> Tokens { get; }
    }
}
