using System;
using System.Reflection;
using AutoStep.Execution.Dependency;

namespace AutoStep.Definitions.Interaction
{
    public class DelegateInteractionMethod : DefinedInteractionMethod
    {
        private readonly object target;

        public DelegateInteractionMethod(string name, object target, MethodInfo method)
            : base(name, method)
        {
            this.target = target;
        }

        public DelegateInteractionMethod(string name, Delegate @delegate)
            : base(name, @delegate.Method)
        {
            target = @delegate.Target;
        }

        protected override object GetMethodTarget(IServiceScope scope)
        {
            return target;
        }
    }
}
