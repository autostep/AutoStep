using System;
using System.Collections.Generic;
using System.Text;
using AutoStep.Elements.Interaction;
using AutoStep.Language.Interaction.Traits;

namespace AutoStep.Language.Interaction
{
    public class InteractionsFileCompilerResult : CompilerResult<InteractionFileElement>
    {
        public InteractionsFileCompilerResult(bool success, InteractionFileElement? output = null)
            : base(success, output)
        {
        }

        public InteractionsFileCompilerResult(bool success, IEnumerable<CompilerMessage> messages, InteractionFileElement? output = null)
            : base(success, messages, output)
        {
        }
    }
}
