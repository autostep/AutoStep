using System;
using System.Collections.Generic;
using AutoStep.Definitions.Interaction;
using AutoStep.Elements.Interaction;
using AutoStep.Execution.Contexts;

namespace AutoStep.Execution.Interaction
{
    public class MethodContext : TestExecutionContext
    {
        public MethodContext()
        {
            Variables = new InteractionVariables();
        }

        public MethodContext(MethodCallElement call, InteractionMethod methodDef, InteractionVariables variables)
        {
            MethodCall = call;
            MethodDefinition = methodDef;
            Variables = variables;
        }

        public object? ChainValue { get; set; }

        public InteractionMethod? MethodDefinition { get; }

        public MethodCallElement? MethodCall { get; }

        public InteractionVariables Variables { get; }
    }
}
