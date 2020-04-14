using AutoStep.Elements.Interaction;

namespace AutoStep.Language.Interaction
{
    /// <summary>
    /// Provides access to a lookup of a definition element to the in-scope method table for that element.
    /// </summary>
    public interface IExtendedMethodTableReferences
    {
        /// <summary>
        /// Gets the in-scope method table for a given interaction element.
        /// </summary>
        /// <param name="definitionElement">The interaction definition.</param>
        /// <returns>The in-scope method table (or null if the element is not in the interaction set).</returns>
        MethodTable? GetMethodTableForElement(InteractionDefinitionElement definitionElement);
    }
}
