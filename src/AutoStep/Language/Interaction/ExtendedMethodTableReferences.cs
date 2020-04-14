using System.Collections.Generic;
using AutoStep.Elements.Interaction;

namespace AutoStep.Language.Interaction
{
    /// <summary>
    /// Container for extended method table references.
    /// </summary>
    internal class ExtendedMethodTableReferences : IExtendedMethodTableReferences
    {
        private readonly Dictionary<InteractionDefinitionElement, MethodTable> definitionElementReference = new Dictionary<InteractionDefinitionElement, MethodTable>();

        /// <inheritdoc/>
        public MethodTable? GetMethodTableForElement(InteractionDefinitionElement definitionElement)
        {
            if (definitionElementReference.TryGetValue(definitionElement, out var methods))
            {
                return methods;
            }

            return null;
        }

        /// <summary>
        /// Add a method table reference for an element.
        /// </summary>
        /// <param name="element">The definition element.</param>
        /// <param name="methodTable">The method table.</param>
        public void AddMethodTableReference(InteractionDefinitionElement element, MethodTable methodTable)
        {
            definitionElementReference[element] = methodTable;
        }
    }
}
