using System;
using System.Collections.Generic;
using System.Text;

namespace AutoStep.Core
{
    [AttributeUsage(AttributeTargets.Class)]
    public class StepsAttribute : Attribute
    {
    }

    public abstract class StepDefinitionAttribute : Attribute
    {
        public string Declaration { get; }

        public StepType Type { get; }

        protected StepDefinitionAttribute(StepType type, string declaration)
        {
            Type = type;
            Declaration = declaration;
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class GivenAttribute : StepDefinitionAttribute
    {
        public GivenAttribute(string declaration)
            : base(StepType.Given, declaration)
        {
        }
    }
}
