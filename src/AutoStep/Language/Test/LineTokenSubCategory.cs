namespace AutoStep.Language
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

        /// <summary>
        /// App Entity
        /// </summary>
        InteractionApp,

        /// <summary>
        /// Trait Entity
        /// </summary>
        InteractionTrait,

        /// <summary>
        /// Component Entity
        /// </summary>
        InteractionComponent,

        /// <summary>
        /// A name.
        /// </summary>
        InteractionName,

        /// <summary>
        /// The inherits property name.
        /// </summary>
        InteractionInherits,

        /// <summary>
        /// A method call separator '->'.
        /// </summary>
        InteractionCallSeparator,

        /// <summary>
        /// A constant reference.
        /// </summary>
        InteractionConstant,

        /// <summary>
        /// A variable reference.
        /// </summary>
        InteractionVariable,

        /// <summary>
        /// An interaction array reference.
        /// </summary>
        InteractionArray,

        /// <summary>
        /// A numeric literal.
        /// </summary>
        InteractionLiteral,

        /// <summary>
        /// $component$
        /// </summary>
        InteractionComponentPlaceholder,

        /// <summary>
        /// Method open or close ( or ).
        /// </summary>
        InteractionParentheses,
    }
}
