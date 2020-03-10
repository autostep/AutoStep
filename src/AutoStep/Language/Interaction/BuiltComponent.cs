using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Autofac.Features.ResolveAnything;
using AutoStep.Elements.Interaction;
using AutoStep.Elements.Parts;
using AutoStep.Language.Interaction.Traits;

namespace AutoStep.Language.Interaction
{

    internal class BuiltComponent
    {
        public BuiltComponent(string name, MethodTable methodTable)
        {
            Name = name;
            MethodTable = methodTable;
        }

        public string Name { get; }

        public MethodTable MethodTable { get; }
    }
}
