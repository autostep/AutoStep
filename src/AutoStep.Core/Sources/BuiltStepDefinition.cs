using System;
using System.Reflection;

namespace AutoStep.Core.Sources
{
    public class BuiltStepDefinition : StepDefinition
    {
        private readonly MethodInfo method;

        public BuiltStepDefinition(AssemblyStepDefinitionSource source, MethodInfo method, StepDefinitionAttribute declaringAttribute)
            : base(source, declaringAttribute?.Type ?? throw new ArgumentNullException(nameof(declaringAttribute)), declaringAttribute.Declaration)
        {
            this.method = method;
        }

        public override bool IsSameDefinition(StepDefinition def)
        {
            // It's the same definition if it's the same method handle.
            if (def is BuiltStepDefinition builtDef)
            {
                return builtDef.method.MethodHandle == method.MethodHandle;
            }

            return false;
        }
    }
}
