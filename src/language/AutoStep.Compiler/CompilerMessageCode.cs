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
        /// An annotation is not valid at this position.
        /// </summary>
        UnexpectedAnnotation =    00001,

        /// <summary>
        /// Only one feature is permitted per file.
        /// </summary>
        OnlyOneFeatureAllowed =   00002,

        /// <summary>
        /// The feature has no scenarios.
        /// </summary>
        NoScenarios =             00003,

        /// <summary>
        /// A step reference is not expected at this point.
        /// </summary>
        StepNotExpected =         00004,

        /// <summary>
        /// An 'And' step reference must be preceded by a Given, When or Then.
        /// </summary>
        AndMustFollowNormalStep = 00005,

        /// <summary>
        /// An option has been specified with a blank setting value.
        /// </summary>
        OptionWithNoSetting =     00006,

        /// <summary>
        /// The scenario keyword is not valid.
        /// </summary>
        InvalidScenarioKeyword =  00007,

        /// <summary>
        /// The feature keyword is not valid.
        /// </summary>
        InvalidFeatureKeyword = 00008,

        /// <summary>
        /// The number of columns in a row does not match the number of columns in the header.
        /// </summary>
        TableColumnsMismatch = 0009,

        /// <summary>
        /// The scenario outline keyword is not valid.
        /// </summary>
        InvalidScenarioOutlineKeyword = 00010,

        /// <summary>
        /// Not expecting an example, because we are not in a scenario outline.
        /// </summary>
        NotExpectingExample = 00011,

        /// <summary>
        /// The examples keyword is not valid.
        /// </summary>
        InvalidExamplesKeyword = 00012,

        /// <summary>
        /// An examples variable insertion has been specified in a regular scenario.
        /// </summary>
        ExampleVariableInScenario = 00013,

        /// <summary>
        /// Example variable has been referenced, but not declared.
        /// </summary>
        ExampleVariableNotDeclared = 00014,

        //// Syntax Errors ////

        /// <summary>
        /// Generic Syntax error found while parsing the token stream.
        /// </summary>
        SyntaxError = 10000,
        NoScenarioTitleProvided = 10001,
        NoScenarioOutlineTitleProvided = 10002,
        UnexpectedEndOfFile = 10003,
        BadTagFormat = 10004,
        BadOptionFormat = 10005,
        ArgumentHasNotBeenClosed = 10006,
        TableRowHasNotBeenTerminated = 10007,
        ExamplesBlockRequiresTable = 10008,
        NoFeatureTitleProvided = 10009,
    }
}
