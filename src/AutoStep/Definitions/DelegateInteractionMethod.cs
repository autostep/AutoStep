using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using AutoStep.Execution.Dependency;

namespace AutoStep.Definitions
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
            this.target = @delegate.Target;
        }

        protected override object GetMethodTarget(IServiceScope scope)
        {
            return target;
        }
    }
}
