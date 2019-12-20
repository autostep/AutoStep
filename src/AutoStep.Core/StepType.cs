namespace AutoStep.Core
{
    /// <summary>
    /// Defines the possible step types.
    /// </summary>
    public enum StepType
    {
        /// <summary>
        /// Given step.
        /// </summary>
        Given,

        /// <summary>
        /// When step.
        /// </summary>
        When,

        /// <summary>
        /// Then step.
        /// </summary>
        Then,

        /// <summary>
        /// And step; bound to one of the other types.
        /// </summary>
        And,
    }
}
