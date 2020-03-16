using AutoStep.Elements.Interaction;

namespace AutoStep.Language.Interaction
{
    /// <summary>
    /// Exposes the functionality to generate an interaction set from a set of files.
    /// </summary>
    public interface IInteractionSetBuilder
    {
        /// <summary>
        /// Add an interaction file to consider during <see cref="Build(IInteractionsConfiguration)"/>.
        /// </summary>
        /// <param name="interactionFile">The file.</param>
        void AddInteractionFile(InteractionFileElement interactionFile);

        /// <summary>
        /// Builds the interaction set.
        /// </summary>
        /// <param name="interactionsConfig">
        /// The interactions config, that provides the root method table,
        /// containing all of the system-provided methods, plus the set of available constants.</param>
        /// <returns>The set build result.</returns>
        /// <remarks>
        /// This method goes through all the full trait graph and component list and determines the actual method table and step set for each
        /// component.
        /// </remarks>
        InteractionSetBuilderResult Build(IInteractionsConfiguration interactionsConfig);
    }
}
