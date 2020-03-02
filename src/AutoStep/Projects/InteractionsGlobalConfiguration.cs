using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AutoStep.Definitions;
using AutoStep.Language;
using AutoStep.Language.Interaction;
using AutoStep.Language.Interaction.Parser;
using AutoStep.Language.Test;
using Microsoft.Extensions.Logging;

namespace AutoStep.Projects
{
    public class InteractionsGlobalConfiguration
    {
        internal MethodTable MethodTable { get; } = new MethodTable();

        public void AddOrReplaceMethod(DefinedInteractionMethod method)
        {
            MethodTable.Set(method);
        }
    }
}
