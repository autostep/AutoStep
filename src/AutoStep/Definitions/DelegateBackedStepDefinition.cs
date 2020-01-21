using System;
using System.Reflection;
using System.Threading.Tasks;
using AutoStep.Execution;
using AutoStep.Execution.Dependency;

namespace AutoStep.Definitions
{
    public class DelegateBackedStepDefinition : MethodBackedStepDefinition
    {
        protected object Target { get; }

        public DelegateBackedStepDefinition(IStepDefinitionSource source, object target, MethodInfo method, StepType type, string declaration)
            : base(source, method, type, declaration)
        {
            Target = target;
        }

        public override bool IsSameDefinition(StepDefinition def)
        {
            if (def is DelegateBackedStepDefinition delDef)
            {
                return delDef.Method.MethodHandle == Method.MethodHandle && delDef.Target == Target;
            }

            return false;
        }

        protected override object GetMethodTarget(IServiceScope scope)
        {
            return Target;
        }
    }
}
