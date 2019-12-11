using System;
using System.Collections.Generic;
using System.Text;

namespace AutoStep.Compiler
{
#pragma warning disable SA1025 // Code should not contain multiple whitespace in a row
    public enum CompilerMessageCode
    {
        SyntaxError =          00001,
        UnexpectedAnnotation = 00002,
        OnlyOneFeatureAllowed = 00003,
        NoScenarios = 00004,
        StepNotExpected = 5,
        AndMustFollowNormalStep = 6,
        OptionWithNoSetting = 7,
        InvalidScenarioKeyword = 8
    }
#pragma warning restore SA1025 // Code should not contain multiple whitespace in a row
}
