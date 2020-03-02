using System;
using System.Collections.Generic;
using AutoStep.Definitions;
using AutoStep.Elements.Interaction;
using AutoStep.Execution.Contexts;

namespace AutoStep.Execution.Interaction
{
    public class MethodContext : TestExecutionContext
    {
        public MethodContext()
        {
        }

        public MethodContext(MethodCallElement call, InteractionMethod methodDef)
        {
            MethodCall = call;
            MethodDefinition = methodDef;
        }

        public object? ChainValue { get; set; }

        public InteractionMethod? MethodDefinition { get; }

        public MethodCallElement? MethodCall { get; }
    }
}
