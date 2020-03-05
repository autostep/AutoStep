using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using AutoStep.Execution.Dependency;

namespace AutoStep.Definitions.Interaction
{
    public class ClassBackedInteractionMethod : DefinedInteractionMethod
    {
        private readonly Type implType;

        public ClassBackedInteractionMethod(string name, Type classType, MethodInfo method)
            : base(name, method)
        {
            implType = classType;
        }

        protected override object GetMethodTarget(IServiceScope scope)
        {
            return scope.Resolve(implType);
        }
    }
}
