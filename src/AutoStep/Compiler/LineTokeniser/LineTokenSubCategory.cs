namespace AutoStep
{
    /// <summary>
    /// Defines the line token sub-categories.
    /// </summary>
    public enum LineTokenSubCategory
    {
        /// <summary>
        /// No applicable sub-category.
        /// </summary>
        None,

        /// <summary>
        /// A tag. @
        /// </summary>
        Tag,

        /// <summary>
        /// An option. $
        /// </summary>
        Option,

        /// <summary>
        /// A step definition.
        /// </summary>
        StepDefine,

        /// <summary>
        /// A feature.
        /// </summary>
        Feature,

        /// <summary>
        /// A background block.
        /// </summary>
        Background,

        /// <summary>
        /// A scenario.
        /// </summary>
        Scenario,

        /// <summary>
        /// A scenario outline.
        /// </summary>
        ScenarioOutline,

        /// <summary>
        /// An examples block.
        /// </summary>
        Examples,

        /// <summary>
        /// A Given step type.
        /// </summary>
        Given,

        /// <summary>
        /// A When step type.
        /// </summary>
        When,

        /// <summary>
        /// A Then step type.
        /// </summary>
        Then,

        /// <summary>
        /// An And step type.
        /// </summary>
        And,

        /// <summary>
        /// A table cell.
        /// </summary>
        Cell,

        /// <summary>
        /// A table header.
        /// </summary>
        Header,

        /// <summary>
        /// Unbound step text.
        /// </summary>
        Unbound,

        /// <summary>
        /// Bound step text.
        /// </summary>
        Bound,

        /// <summary>
        /// A step declaration.
        /// </summary>
        Declaration,

        /// <summary>
        /// A variable reference inside an argument.
        /// </summary>
        ArgumentVariable,

        /// <summary>
        /// A description block.
        /// </summary>
        Description,
    }
}
