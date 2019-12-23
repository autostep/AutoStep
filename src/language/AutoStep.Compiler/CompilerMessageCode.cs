using System;
using System.Collections.Generic;
using System.Text;

namespace AutoStep.Compiler
{
#pragma warning disable SA1025 // Code should not contain multiple whitespace in a row

    /// <summary>
    /// Defines the compiler message codes.
    /// </summary>
    public enum CompilerMessageCode
    {
        /// <summary>
        /// Syntax error found while parsing the token stream.
        /// </summary>
        SyntaxError =             00001,

        /// <summary>
        /// An annotation is not valid at this position.
        /// </summary>
        UnexpectedAnnotation =    00002,

        /// <summary>
        /// Only one feature is permitted per file.
        /// </summary>
        OnlyOneFeatureAllowed =   00003,

        /// <summary>
        /// The feature has no scenarios.
        /// </summary>
        NoScenarios =             00004,

        /// <summary>
        /// A step reference is not expected at this point.
        /// </summary>
        StepNotExpected =         00005,

        /// <summary>
        /// An 'And' step reference must be preceded by a Given, When or Then.
        /// </summary>
        AndMustFollowNormalStep = 00006,

        /// <summary>
        /// An option has been specified with a blank setting value.
        /// </summary>
        OptionWithNoSetting =     00007,

        /// <summary>
        /// The scenario keyword is not valid.
        /// </summary>
        InvalidScenarioKeyword =  00008,

        /// <summary>
        /// The feature keyword is not valid.
        /// </summary>
        InvalidFeatureKeyword = 00009,
        TableColumnsMismatch = 10,
    }
}
