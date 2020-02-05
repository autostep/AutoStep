using System;
using System.Collections.Generic;
using System.Text;

namespace AutoStep.Language.Interaction
{
    public class BuiltInteractionsFile 
    {

    }

    public class InteractionsFileCompilerResult : CompilerResult<BuiltInteractionsFile>
    {
        public InteractionsFileCompilerResult(bool success, BuiltInteractionsFile? output = null) 
            : base(success, output)
        {
        }

        public InteractionsFileCompilerResult(bool success, IEnumerable<CompilerMessage> messages, BuiltInteractionsFile? output = null)
            : base(success, messages, output)
        {
        }
    }
}
