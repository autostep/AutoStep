namespace AutoStep.Language
{
    /// <summary>
    /// Defines the possible line token categories.
    /// </summary>
    public enum LineTokenCategory
    {
        /// <summary>
        /// Annotation, e.g. Tag/Option.
        /// </summary>
        Annotation,

        /// <summary>
        /// A step type keyword (Given/When/Then/And).
        /// </summary>
        StepTypeKeyword,

        /// <summary>
        /// The entrance to a block, such as Feature:, Scenario:, Step: etc.
        /// </summary>
        EntryMarker,

        /// <summary>
        /// A comment.
        /// </summary>
        Comment,

        /// <summary>
        /// An entity name, like the name of a scenario or feature.
        /// </summary>
        EntityName,

        /// <summary>
        /// A table border character '|'.
        /// </summary>
        TableBorder,

        /// <summary>
        /// Description free-text.
        /// </summary>
        Text,

        /// <summary>
        /// A variable reference.
        /// </summary>
        Variable,

        /// <summary>
        /// Step body text.
        /// </summary>
        StepText,

        /// <summary>
        /// A bound argument value.
        /// </summary>
        BoundArgument,

        /// <summary>
        /// The left-hand side of an interaction property name, e.g. 'name:', 'traits:', 'inherits:'.
        /// </summary>
        InteractionPropertyName,

        /// <summary>
        /// needs-defining keyword.
        /// </summary>
        InteractionNeedsDefining,

        /// <summary>
        /// A name (method, trait name, component name, etc).
        /// </summary>
        InteractionName,

        /// <summary>
        /// A separator character of some form.s
        /// </summary>
        InteractionSeparator,

        /// <summary>
        /// A literal string.
        /// </summary>
        InteractionString,

        /// <summary>
        /// An argument to a method.
        /// </summary>
        InteractionArguments,

        /// <summary>
        /// A step placeholder.
        /// </summary>
        Placeholder,
    }
}
