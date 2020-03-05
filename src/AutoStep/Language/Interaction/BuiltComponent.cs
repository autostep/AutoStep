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
        public string Name { get; set; }

        public MethodTable MethodTable { get; set; }
    }
}
