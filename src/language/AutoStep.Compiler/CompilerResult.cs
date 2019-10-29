using AutoStep.Core;
using System.Collections.Generic;

namespace AutoStep.Compiler
{
    public class CompilerResult
    {
        public bool Success { get; set; }

        public BuiltFile Output { get; set; }

        public IEnumerable<CompilerMessage> Messages { get; set; }
    }
}
