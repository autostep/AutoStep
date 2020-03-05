using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoStep.Definitions.Interaction;
using AutoStep.Elements.Interaction;

namespace AutoStep.Language.Interaction
{
    public class MethodTable
    {
        private Dictionary<string, InteractionMethod>? copyFrom;
        private Dictionary<string, InteractionMethod>? methods;

        public MethodTable()
        {
            methods = new Dictionary<string, InteractionMethod>();
        }

        public MethodTable(MethodTable original)
        {
            copyFrom = original.methods ?? original.copyFrom;
        }

        public void Set(string name, MethodDefinitionElement methodDef)
        {
            if (methodDef is null)
            {
                throw new ArgumentNullException(nameof(methodDef));
            }

            Set(new FileDefinedInteractionMethod(name)
            {
                NeedsDefining = methodDef.NeedsDefining,
                MethodDefinition = methodDef,
            });
        }

        public void Set(InteractionMethod predefinedMethod)
        {
            if (methods is null)
            {
                methods = new Dictionary<string, InteractionMethod>(copyFrom);
                copyFrom = null;
            }

            methods[predefinedMethod.Name] = predefinedMethod;
        }

        public bool TryGetMethod(string name, out InteractionMethod method)
        {
            return (methods ?? copyFrom)!.TryGetValue(name, out method);
        }

        public IEnumerable<KeyValuePair<string, InteractionMethod>> Methods => methods ?? copyFrom ?? Enumerable.Empty<KeyValuePair<string, InteractionMethod>>();
    }
}
