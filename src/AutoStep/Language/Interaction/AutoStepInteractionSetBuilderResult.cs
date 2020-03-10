using System.Collections.Generic;

namespace AutoStep.Language.Interaction
{
    internal class AutoStepInteractionSetBuilderResult : CompilerResult<AutoStepInteractionSet>
    {
        public AutoStepInteractionSetBuilderResult(bool success, IEnumerable<CompilerMessage> messages, AutoStepInteractionSet? output = null) 
            : base(success, messages, output)
        {
        }
    }
}
