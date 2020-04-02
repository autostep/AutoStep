namespace AutoStep.Language.Interaction
{
    /// <summary>
    /// Defines an interface for the interaction system configuration; this exposes the root method table and available constants.
    /// </summary>
    public interface IInteractionsConfiguration
    {
        /// <summary>
        /// Gets the root method table that contains all top-level methods available to the interaction system.
        /// </summary>
        RootMethodTable RootMethodTable { get; }

        /// <summary>
        /// Gets the set of available constants that can be used in the interaction system.
        /// </summary>
        InteractionConstantSet Constants { get; }
    }
}
