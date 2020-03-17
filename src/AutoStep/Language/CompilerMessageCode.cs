namespace AutoStep.Language
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

        /// <summary>
        /// Step variable has been reference, but not declared.
        /// </summary>
        StepVariableNotDeclared = 00015,

        /// <summary>
        /// Cannot specify a dynamic value in a step definition.
        /// </summary>
        CannotSpecifyDynamicValueInStepDefinition = 00016,

        /// <summary>
        /// Cannot use an empty parameter as a variable name for a step definition.
        /// </summary>
        StepVariableNameRequired = 00017,

        /// <summary>
        /// A scenario name has been duplicated.
        /// </summary>
        DuplicateScenarioNames = 00018,

        //// Syntax Errors ////

        /// <summary>
        /// Generic Syntax error found while parsing the token stream.
        /// </summary>
        SyntaxError = 10000,

        /// <summary>
        /// A scenario has been defined, without a title.
        /// </summary>
        NoScenarioTitleProvided = 10001,

        /// <summary>
        /// A scenario outline has been defined, without a title.
        /// </summary>
        NoScenarioOutlineTitleProvided = 10002,

        /// <summary>
        /// The end-of-file was reached, but we were expecting more content.
        /// </summary>
        UnexpectedEndOfFile = 10003,

        /// <summary>
        /// A statement argument has not been closed.
        /// </summary>
        ArgumentHasNotBeenClosed = 10004,

        /// <summary>
        /// A table row has not been terminated.
        /// </summary>
        TableRowHasNotBeenTerminated = 10005,

        /// <summary>
        /// An examples block has been specified, but with no table.
        /// </summary>
        ExamplesBlockRequiresTable = 10006,

        /// <summary>
        /// A feature has been defined, but no title has been provided.
        /// </summary>
        NoFeatureTitleProvided = 10007,

        /// <summary>
        /// Cannot use the 'And' keyword for step definition statements.
        /// </summary>
        InvalidStepDefineKeyword = 10008,

        /// <summary>
        /// Step variable contains invalid whitespace.
        /// </summary>
        StepVariableInvalidWhitespace = 10009,

        /// <summary>
        /// Annotation (@ or $) found in the middle of an annotation.
        /// </summary>
        UnexpectedAnnotationMarker = 10010,

        /// <summary>
        /// Can't have whitespace here.
        /// </summary>
        UnexpectedAnnotationWhiteSpace = 10011,

        //// Linker Messages ////

        /// <summary>
        /// A step could not be bound.
        /// </summary>
        LinkerNoMatchingStepDefinition = 20001,

        /// <summary>
        /// Multiple bindings were found for a step.
        /// </summary>
        LinkerMultipleMatchingDefinitions = 20002,

        /// <summary>
        /// The hinted argument type requires a value.
        /// </summary>
        TypeRequiresValueForArgument = 20003,

        /// <summary>
        /// The hinted argument type is not compatible with the bound step reference.
        /// </summary>
        ArgumentTypeNotCompatible = 20004,

        //// Errors from the Interaction Language ////

        /// <summary>
        /// A name has already been provided for this entity.
        /// </summary>
        InteractionNameAlreadySet = 30001,

        /// <summary>
        /// The requested constant is not defined.
        /// </summary>
        InteractionConstantNotDefined = 30002,

        /// <summary>
        /// A duplicate trait has been specified in a trait definition list, either when naming a combination trait,
        /// or when defining a component's traits.
        /// </summary>
        InteractionDuplicateTrait = 30003,

        /// <summary>
        /// The specified variable is not defined.
        /// </summary>
        InteractionVariableNotDefined = 30004,

        /// <summary>
        /// The specified variable exists, but is not an array type.
        /// </summary>
        InteractionVariableNotAnArray = 30005,

        /// <summary>
        /// Entity name has not been provided.
        /// </summary>
        InteractionMissingExpectedName = 30006,

        /// <summary>
        /// An interaction method referenced by a component method or step is not defined.
        /// </summary>
        InteractionMethodRequiredButNotDefined = 30007,

        /// <summary>
        /// The specified interaction method is not available.
        /// </summary>
        InteractionMethodNotAvailable = 30008,

        /// <summary>
        /// The specified interaction method does not have the same number of arguments as the call.
        /// </summary>
        InteractionMethodArgumentMismatch = 30009,

        /// <summary>
        /// Interaction methods cannot call themselves.
        /// </summary>
        InteractionMethodCircularReference = 30010,

        /// <summary>
        /// A step definition inside a trait has not specified a $component$ placeholder.
        /// </summary>
        InteractionTraitStepDefinitionMustHaveComponent = 30011,

        /// <summary>
        /// A step definition inside a component has specified a $component$ placeholder.
        /// </summary>
        InteractionComponentStepDefinitionCannotHaveComponentMarker = 30012,

        /// <summary>
        /// An interaction method is not available; custom message to indicate it can be created as needs-defining.
        /// </summary>
        InteractionMethodNotAvailablePermitUndefined = 30013,

        /// <summary>
        /// A method is required by a component because of the traits it has applied,
        /// but has not been defined by the component.
        /// </summary>
        InteractionMethodFromTraitRequiredButNotDefined = 30014,

        /// <summary>
        /// Detected an direct or indirect component inheritance loop.
        /// </summary>
        InteractionComponentInheritanceLoop = 30015,

        /// <summary>
        /// Interaction step missing the declaration.
        /// </summary>
        InteractionMissingStepDeclaration = 30016,

        /// <summary>
        /// Interaction method call requires parentheses.
        /// </summary>
        InteractionMethodNeedsParentheses = 30017,

        /// <summary>
        /// Interaction block contains invalid content.
        /// </summary>
        InteractionInvalidContent = 30018,

        /// <summary>
        /// Interaction method has an unterminated declaration.
        /// </summary>
        InteractionMethodDeclUnterminated = 30019,

        /// <summary>
        /// Missing a parameter.
        /// </summary>
        InteractionMethodDeclMissingParameter = 30020,

        /// <summary>
        /// Missing a parameter separator.
        /// </summary>
        InteractionMethodDeclMissingParameterSeparator = 30021,

        /// <summary>
        /// Unexpected content in a method declaration.
        /// </summary>
        InteractionMethodDeclUnexpectedContent = 30022,

        /// <summary>
        /// Unterminated string.
        /// </summary>
        InteractionUnterminatedString = 30023,

        /// <summary>
        /// Method call has not been closed.
        /// </summary>
        InteractionMethodCallUnterminated = 30024,

        /// <summary>
        /// Missing method call separator (->).
        /// </summary>
        InteractionMethodCallMissingSeparator = 30025,

        /// <summary>
        /// Missing method call parameter separator.
        /// </summary>
        InteractionMethodCallMissingParameterSeparator = 30026,

        /// <summary>
        /// Specified component to inherit from cannot be found.
        /// </summary>
        InteractionComponentInheritedComponentNotFound = 30027,

        //// Errors from Exceptions ////

        /// <summary>
        /// IO Problem occurred while compiling.
        /// </summary>
        IOException = 90001,

        /// <summary>
        /// Unknown error category (the catch-all).
        /// </summary>
        UncategorisedException = 90002,
    }
}
