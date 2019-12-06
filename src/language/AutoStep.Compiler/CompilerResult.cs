using System.Collections.Generic;
using System.Linq;
using AutoStep.Core;

namespace AutoStep.Compiler
{
    public class CompilerResult
    {
        public CompilerResult(bool success, BuiltFile? output = null)
        {
            Success = success;
            Messages = Enumerable.Empty<CompilerMessage>();
            Output = output;
        }

        public CompilerResult(bool success, IEnumerable<CompilerMessage> messages, BuiltFile? output = null)
        {
            Success = success;
            // Freeze the messages
            Messages = messages.ToArray();
            Output = output;
        }

        public bool Success { get; set; }

        public BuiltFile? Output { get; set; }

        public IEnumerable<CompilerMessage> Messages { get; }
    }
}
