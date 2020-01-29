namespace AutoStep
{
    public enum LineTokenCategory
    {
        Annotation,
        StepTypeKeyword,

        /// <summary>
        /// The entrance to a block, such as Feature:, Scenario:, Step: etc.
        /// </summary>
        EntryMarker,

        /// <summary>
        /// The declaration of an argument in a Step Definition.
        /// </summary>
        ArgumentDeclaration,

        /// <summary>
        /// Non-argument text content.
        /// </summary>
        TextDeclaration,
        Comment,
        EntityName,
        TableBorder,
        Text,
        Variable,
        StepText,
        BoundArgument
    }
}
