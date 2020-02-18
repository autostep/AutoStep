using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Autofac.Features.ResolveAnything;
using AutoStep.Elements.Interaction;
using AutoStep.Elements.Parts;
using AutoStep.Language.Interaction.Parser;
using AutoStep.Language.Interaction.Traits;

namespace AutoStep.Language.Interaction
{
    internal class AutoStepInteractionGroupBuilderResult : CompilerResult<AutoStepInteractionSet>
    {
        public AutoStepInteractionGroupBuilderResult(bool success, IEnumerable<CompilerMessage> messages, AutoStepInteractionSet? output = null) 
            : base(success, messages, output)
        {
        }
    }
}
