namespace AutoStep.Execution
{
    /// <summary>
    /// Represents the set of interaction variables currently in scope.
    /// </summary>
    /// <remarks>
    /// Interaction variables can be any type, so we're basing off of object.
    /// This will lead to boxing for value types, but not worried about the performance of that just yet.
    /// </remarks>
    public class InteractionVariables : VariableSetBase<object>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InteractionVariables"/> class.
        /// </summary>
        public InteractionVariables()
            : base(false)
        {
        }
    }
}
