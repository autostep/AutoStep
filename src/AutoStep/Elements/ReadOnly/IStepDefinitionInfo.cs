using System;
using System.Collections.Generic;
using System.Text;
using AutoStep.Compiler;
using AutoStep.Elements.StepTokens;

namespace AutoStep.Elements.ReadOnly
{
    public interface IStepDefinitionInfo : IElementInfo
    {
        StepType Type { get; }

        string Declaration { get; }

        string? Description { get; }

        IReadOnlyList<IAnnotationInfo> Annotations { get; }
    }
}
