using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AutoStep.Definitions.Interaction;
using AutoStep.Elements.Interaction;

namespace AutoStep.Language.Interaction
{
    /// <summary>
    /// Represents the table of available methods for a given entity, indexed by name.
    /// </summary>
    public class MethodTable
    {
        private Dictionary<string, InteractionMethod>? copyFrom;
        private Dictionary<string, InteractionMethod>? methods;

        /// <summary>
        /// Initializes a new instance of the <see cref="MethodTable"/> class.
        /// </summary>
        public MethodTable()
        {
            methods = new Dictionary<string, InteractionMethod>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MethodTable"/> class.
        /// </summary>
        /// <param name="original">The original method table to copy from (copy-on-write).</param>
        public MethodTable(MethodTable original)
        {
            if (original is null)
            {
                throw new ArgumentNullException(nameof(original));
            }

            copyFrom = original.methods ?? original.copyFrom;
        }

        /// <summary>
        /// Gets the set of the methods available in this method table.
        /// </summary>
        public IEnumerable<KeyValuePair<string, InteractionMethod>> Methods => methods ?? copyFrom ?? Enumerable.Empty<KeyValuePair<string, InteractionMethod>>();

        /// <summary>
        /// Insert a method definition element (from a file) into the method table.
        /// </summary>
        /// <param name="name">The name of the method.</param>
        /// <param name="methodDef">The parsed method definition.</param>
        public void SetMethod(string name, MethodDefinitionElement methodDef)
        {
            if (methodDef is null)
            {
                throw new ArgumentNullException(nameof(methodDef));
            }

            SetMethod(new FileDefinedInteractionMethod(name, methodDef)
            {
                NeedsDefining = methodDef.NeedsDefining,
            });
        }

        /// <summary>
        /// Insert a method definition (pre-defined) into the method table.
        /// </summary>
        /// <param name="predefinedMethod">The method definition.</param>
        public virtual void SetMethod(InteractionMethod predefinedMethod)
        {
            if (predefinedMethod is null)
            {
                throw new ArgumentNullException(nameof(predefinedMethod));
            }

            if (methods is null)
            {
                methods = new Dictionary<string, InteractionMethod>(copyFrom);
                copyFrom = null;
            }

            methods[predefinedMethod.Name] = predefinedMethod;
        }

        /// <summary>
        /// Attempts to retrieve a method from the method table.
        /// </summary>
        /// <param name="name">The name of the method.</param>
        /// <param name="method">The method implementation.</param>
        /// <returns>True if the method exists, false otherwise.</returns>
        public bool TryGetMethod(string name, [NotNullWhen(true)] out InteractionMethod? method)
        {
            return (methods ?? copyFrom)!.TryGetValue(name, out method);
        }
    }
}
