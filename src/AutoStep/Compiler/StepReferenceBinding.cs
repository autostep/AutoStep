using System;
using System.Collections.Generic;
using System.Text;
using AutoStep.Definitions;

namespace AutoStep.Compiler
{
    public class StepReferenceBinding
    {
        public StepReferenceBinding(StepDefinition def, ArgumentBinding[]? args)
        {
            Definition = def;
            Arguments = args;
        }

        public StepDefinition Definition { get; }

        public ArgumentBinding[]? Arguments { get; }
    }

}
