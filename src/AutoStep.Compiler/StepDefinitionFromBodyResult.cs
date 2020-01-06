using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoStep.Compiler.Parser;
using AutoStep.Core;
using AutoStep.Core.Elements;
using AutoStep.Core.Sources;

namespace AutoStep.Compiler
{
    public class StepDefinitionFromBodyResult : CompilerResult
    {
        public StepDefinitionFromBodyResult(bool success, IEnumerable<CompilerMessage> messages, StepDefinitionElement? element)
            : base(success, messages)
        {
            StepDefinition = element;
        }

        public StepDefinitionElement? StepDefinition { get; }
    }
}
