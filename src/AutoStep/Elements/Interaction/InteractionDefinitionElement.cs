using System;
using System.Collections.Generic;
using System.Text;
using AutoStep.Language;

namespace AutoStep.Elements.Interaction
{
    public abstract class MethodArgumentElement : PositionalElement
    {
    }

    public class StringMethodArgumentElement : MethodArgumentElement
    {
        public string Text { get; set; }

        internal TokenisedArgumentValue Tokenised { get; set; }
    }

    public class VariableRefMethodArgumentElement : MethodArgumentElement
    {
        public string VariableName { get; set; }
    }

    public class VariableArrayRefMethodArgument : MethodArgumentElement
    {
        public string VariableName { get; set; }

        public string ArrayIndex { get; set; }
    }

    public class ConstantMethodArgument : MethodArgumentElement
    {
        public string ConstantName { get; set; }
    }

    public class IntMethodArgument : MethodArgumentElement
    {
        public int Value { get; set; }
    }

    public class FloatMethodArgument : MethodArgumentElement
    {
        public double Value { get; set; }
    }

    public class MethodCallElement : PositionalElement
    {
        public string MethodName { get; set; }

        public List<MethodArgumentElement> Arguments { get; } = new List<MethodArgumentElement>();
    }

    public class MethodDefinitionElement : PositionalElement
    {
    }

    public interface IMethodCallSource
    {
        public List<MethodCallElement> MethodCallChain { get; }
    }

    public class InteractionStepDefinitionElement : StepDefinitionElement, IMethodCallSource
    {
        public List<MethodCallElement> MethodCallChain { get; } = new List<MethodCallElement>();
    }

    public abstract class InteractionDefinitionElement : BuiltElement
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public List<MethodDefinitionElement> Methods { get; }
    }

    public class TraitDefinitionElement : InteractionDefinitionElement
    {
    }

    public class ComponentDefinitionElement : InteractionDefinitionElement
    {
        public string[] Traits { get; set; }
    }
}
