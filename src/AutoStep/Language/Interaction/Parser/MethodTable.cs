using System;
using System.Collections.Generic;
using System.Text;
using AutoStep.Elements.Interaction;

namespace AutoStep.Language.Interaction.Parser
{
    public class InteractionMethod
    {
        public string Name { get; set; }
    }

    public class FileDefinedInteractionMethod : InteractionMethod
    {
        public MethodDefinitionElement MethodDefinition { get; set; }
    }


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
            if (methods == null)
            {
                methods = new Dictionary<string, InteractionMethod>(copyFrom);
            }

            methods[name] = new FileDefinedInteractionMethod
            {
                Name = name,
                MethodDefinition = methodDef,
            };
        }

        public bool TryGetMethod(string name, out InteractionMethod method)
        {
            return (methods ?? copyFrom)!.TryGetValue(name, out method);
        }
    }
}










