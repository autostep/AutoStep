using System;
using System.Collections.Generic;
using System.Text;
using AutoStep.Language;

namespace AutoStep.Elements.Interaction
{

    public class MethodCallElement : PositionalElement
    {
        public string MethodName { get; set; }

        public List<MethodArgumentElement> Arguments { get; } = new List<MethodArgumentElement>();
    }
}
