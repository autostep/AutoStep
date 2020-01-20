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

        protected override Task InvokeMethod(IServiceScope scope, object[] args)
        {
            var arrayWithServiceScope = new object[args.Length + 1];

            arrayWithServiceScope[0] = scope;

            args.CopyTo(arrayWithServiceScope, 1);

            return InvokeInstanceMethod(scope, Target, arrayWithServiceScope);
        }
    }
}
